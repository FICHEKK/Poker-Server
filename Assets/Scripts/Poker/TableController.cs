using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Dao;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;
using RequestProcessors;
using Timer = System.Timers.Timer;

namespace Poker
{
    /// <summary>
    /// Controls and manages a single table. Also handles client's requests and
    /// updates table's state accordingly.
    /// </summary>
    public abstract class TableController
    {
        /// <summary>A blocking queue used to store unprocessed client requests.</summary>
        public BlockingCollection<IClientRequestProcessor> RequestProcessors { get; } = new BlockingCollection<IClientRequestProcessor>();

        /// <summary>A dummy action that is used to indicate the end of the consuming.</summary>
        private readonly IClientRequestProcessor _poisonPill = new PoisonPill();

        /// <summary>Table managed by this controller.</summary>
        protected readonly Table Table;

        /// <summary>Table's title (name).</summary>
        public string Title { get; }

        /// <summary>Table's small blind.</summary>
        public int SmallBlind { get; }

        /// <summary>Indicates whether the table is ranked.</summary>
        public abstract bool IsRanked { get; }

        /// <summary> Indicates whether the table cannot be joined. </summary>
        public bool IsLocked { get; protected set; }

        /// <summary>Table's current number of players.</summary>
        public int PlayerCount => Table.PlayerCount;

        /// <summary>Table's maximum number of players.</summary>
        public int MaxPlayers => Table.MaxPlayers;
        
        /// <summary>The current state of the round (table).</summary>
        private Round _round;
        
        /// <summary>The deck used by this controller to deal cards.</summary>
        private readonly Deck _deck = new Deck();
        
        /// <summary> Timer used to time player's turn durations. </summary>
        private readonly Timer _decisionTimer = new Timer {AutoReset = false};

        protected TableController(Table table, string title, int smallBlind)
        {
            Table = table;
            Title = title;
            SmallBlind = smallBlind;

            InitializeTimer();
            new Thread(ConsumeClientRequests).Start();
        }
        
        private void InitializeTimer()
        {
            _decisionTimer.Interval = (TableConstant.PlayerTurnDuration + TableConstant.PlayerTurnOvertime) * 1000;
            _decisionTimer.Elapsed += (sender, e) =>
            {
                if (_round == null) return;
                if (_round.CurrentPhase == Round.Phase.OnePlayerLeft) return;
                if (_round.CurrentPhase == Round.Phase.Showdown) return;
                
                PlayerFold();
            };
        }
        
        private void ConsumeClientRequests()
        {
            while (true)
            {
                var processor = RequestProcessors.Take();
                if (processor == _poisonPill) return;
                processor.ProcessRequest();
            }
        }
        
        //===========================================================
        //                    Dealer stuff
        //===========================================================
        
        protected void StartNewRound()
        {
            _deck.Shuffle();
            Table.IncrementButtonIndex();

            var smallBlindIndex = Table.GetNextOccupiedSeatIndex(Table.DealerButtonIndex);
            var bigBlindIndex = Table.GetNextOccupiedSeatIndex(smallBlindIndex);
            var playerIndex = Table.GetNextOccupiedSeatIndex(bigBlindIndex);

            _round = new Round(Table.GetPlayerArray(), smallBlindIndex, bigBlindIndex, playerIndex, SmallBlind);
            _round.RoundPhaseChanged += RoundPhaseChangedEventHandler;
            _round.CurrentPlayerChanged += CurrentPlayerChangedEventHandler;
            
            BroadcastBlindsData(smallBlindIndex, bigBlindIndex);
            DealHandCards();
            _round.Start();
        }
        
        private void BroadcastBlindsData(int smallBlindIndex, int bigBlindIndex)
        {
            new Client.Package(Table.GetActiveClients())
                .Append(ServerResponse.Blinds)
                .Append(_round.JustJoinedPlayerIndexes.Count)
                .Append(_round.JustJoinedPlayerIndexes, index => index)
                .Append(Table.DealerButtonIndex)
                .Append(smallBlindIndex)
                .Append(bigBlindIndex)
                .Send();
        }

        private void DealHandCards()
        {
            foreach (var player in Table)
            {
                var handCard1 = _deck.GetNextCard();
                var handCard2 = _deck.GetNextCard();

                player.SetHand(handCard1, handCard2);

                new Client.Package(player.Client)
                    .Append(ServerResponse.Hand)
                    .Append(handCard1)
                    .Append(handCard2)
                    .Send();
            }
        }
        
        private static List<TablePlayer> DetermineWinners(List<TablePlayer> players, List<Card> communityCards, out Hand bestHand)
        {
            if(players == null) throw new ArgumentNullException(nameof(players));
            if(communityCards == null) throw new ArgumentNullException(nameof(communityCards));
            if(players.Count == 0) throw new ArgumentException("Player collection must be non-empty.");
            if(communityCards.Count != 5) throw new ArgumentException("Expected all 5 community cards.");
            
            var winners = new List<TablePlayer>();

            bestHand = null;

            foreach (var player in players)
            {
                var evaluator = new SevenCardEvaluator(player.FirstHandCard, player.SecondHandCard,
                    communityCards[0], communityCards[1], communityCards[2], communityCards[3], communityCards[4]);

                if (bestHand == null)
                {
                    bestHand = evaluator.BestHand;
                    winners.Add(player);
                    continue;
                }

                var result = bestHand.CompareTo(evaluator.BestHand);

                if (result < 0)
                {
                    winners.Clear();
                    winners.Add(player);
                    bestHand = evaluator.BestHand;
                }
                else if (result == 0)
                {
                    winners.Add(player);
                }
            }

            return winners;
        }
        
        //===========================================================
        //                    Event handlers
        //===========================================================
        
        private void RoundPhaseChangedEventHandler(object sender, RoundPhaseChangedEventArgs e)
        {
            switch (e.CurrentPhase)
            {
                case Round.Phase.Flop: RevealCommunityCards(ServerResponse.Flop, 3); break;
                case Round.Phase.Turn: RevealCommunityCards(ServerResponse.Turn, 1); break;
                case Round.Phase.River: RevealCommunityCards(ServerResponse.River, 1); break;
                case Round.Phase.Showdown: ProcessShowdown(); break;
                case Round.Phase.OnePlayerLeft: ProcessOnePlayerLeft(); break;
            }
        }
        
        private void CurrentPlayerChangedEventHandler(object sender, CurrentPlayerChangedEventArgs e)
        {
            SendBroadcastPackage(ServerResponse.PlayerIndex, e.CurrentPlayerIndex);

            new Client.Package(Table[e.CurrentPlayerIndex].Client)
                .Append(ServerResponse.RequiredBet)
                .Append(e.RequiredCall)
                .Append(e.MinRaise)
                .Append(e.MaxRaise)
                .Send();

            _decisionTimer.Stop();
            _decisionTimer.Start();
        }
        
        //===========================================================
        //                    Round phases
        //===========================================================
        
        private void RevealCommunityCards(ServerResponse response, int cardCount)
        {
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(response);

            for (var i = 0; i < cardCount; i++)
            {
                var card = _deck.GetNextCard();
                package.Append(card);
                _round.AddCommunityCard(card);
            }
            
            package.Send();
            Thread.Sleep(TableConstant.PausePerCardDuration * cardCount);
        }
        
        private void ProcessOnePlayerLeft()
        {
            var winner = _round.ActivePlayers[0];
            SendBroadcastPackage(ServerResponse.Showdown, 1, _round.Pot, 1, winner.Index, string.Empty);

            winner.Stack += _round.Pot;
            winner.ChipCount += _round.Pot;
            
            FinishRound(TableConstant.RoundFinishPauseDuration, new HashSet<TablePlayer>{winner});
        }
        
        private void ProcessShowdown()
        {
            RevealActivePlayersCards();

            var sidePots = Pot.CalculateSidePots(_round.ParticipatingPlayers);
            var winningPlayers = new HashSet<TablePlayer>();

            var package = new Client.Package(Table.GetActiveClients())
                .Append(ServerResponse.Showdown)
                .Append(sidePots.Count);

            for (var i = sidePots.Count - 1; i >= 0; i--)
            {
                var sidePotWinners = DetermineWinners(sidePots[i].Contenders, _round.CommunityCards, out var bestHand);
                var winAmount = sidePots[i].Value / sidePotWinners.Count;
                
                foreach (var player in sidePotWinners)
                {
                    player.Stack += winAmount;
                    player.ChipCount += winAmount;
                    winningPlayers.Add(player);
                }
                
                package.Append(sidePots[i].Value)
                       .Append(sidePotWinners.Count)
                       .Append(sidePotWinners, winner => winner.Index)
                       .Append(bestHand.ToStringPretty());
            }
            
            package.Send();
            
            FinishRound(TableConstant.RoundFinishPauseDuration + sidePots.Count * 1000, winningPlayers);
        }
        
        private void RevealActivePlayersCards()
        {
            var package = new Client.Package(Table.GetActiveClients())
                .Append(ServerResponse.CardsReveal)
                .Append(_round.ActivePlayers.Count);
            
            foreach (var player in _round.ActivePlayers)
            {
                package.Append(player.Index)
                       .Append(player.FirstHandCard)
                       .Append(player.SecondHandCard);
            }
            
            package.Send();
        }
        
        private void FinishRound(int timeout, HashSet<TablePlayer> winningPlayers)
        {
            foreach (var player in winningPlayers)
                DaoProvider.Dao.SetWinCount(player.Username, DaoProvider.Dao.GetWinCount(player.Username) + 1);

            foreach (var participant in _round.ParticipatingPlayers)
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            
            Thread.Sleep(timeout);
            KickBrokePlayers();
            OnRoundFinished();
            
            SendBroadcastPackage(ServerResponse.RoundFinished);
        }
        
        private void KickBrokePlayers()
        {
            foreach (var player in Table)
            {
                if (player.Stack > 0) continue;
                Kick(player);
            }
        }
        
        //===========================================================
        //                    Player requests
        //===========================================================

        public void PlayerJoin(Client client, int stack)
        {
            SendTableState(client);

            var lobbyPlayer = Casino.GetLobbyPlayer(client.Username);
            Casino.RemoveLobbyPlayer(lobbyPlayer);
            
            var tablePlayer = new TablePlayer(client, lobbyPlayer.ChipCount, this, stack);
            Casino.AddTablePlayer(tablePlayer);
            Table.AddPlayer(tablePlayer);
            
            SendBroadcastPackage(ServerResponse.PlayerJoined, tablePlayer.Index, tablePlayer.Username, tablePlayer.Stack);
            OnPlayerJoined();
        }
        
        private void SendTableState(Client client)
        {
            var package = new Client.Package(client)
                .Append(ServerResponse.TableState)
                .Append(Table.DealerButtonIndex)
                .Append(SmallBlind)
                .Append(Table.MaxPlayers);
            
            AppendPlayerList(package);

            if (_round == null)
            {
                package.Append(0); // community card count
                package.Append(-1); // player index
                package.Append(0); // pot
            }
            else
            {
                package.Append(_round.CommunityCards.Count);
                package.Append(_round.CommunityCards, card => card);
                package.Append(_round.CurrentPlayerIndex);
                package.Append(_round.Pot);
            }
            
            package.Send();
        }

        private void AppendPlayerList(Client.Package package)
        {
            package.Append(Table.PlayerCount);

            foreach (var player in Table)
            {
                package.Append(player.Index)
                       .Append(player.Username)
                       .Append(player.Stack)
                       .Append(player.Bet)
                       .Append(player.Folded);
            }
        }

        // TODO override this in ranked controller
        public virtual void PlayerLeave(TablePlayer player)
        {
            new Client.Package(player.Client)
                .Append(ServerResponse.LeaveTable)
                .Append(ServerResponse.LeaveTableGranted)
                .Send();
            
            Casino.RemoveTablePlayer(player);
            Table.RemovePlayer(player);
            
            Casino.AddLobbyPlayer(new LobbyPlayer(player.Client, player.ChipCount));
            
            SendBroadcastPackage(ServerResponse.PlayerLeft, player.Index);
            


            _round?.PlayerLeft(player.Index);

            if (Table.PlayerCount == 1)
            {
                _round = null;
            }
        }

        public void PlayerCheck()
        {
            SendBroadcastPackage(ServerResponse.PlayerChecked, _round.CurrentPlayerIndex);
            _round.PlayerChecked();
        }

        public void PlayerCall(int callAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerCalled, _round.CurrentPlayerIndex, callAmount);
            _round.PlayerCalled(callAmount);
        }

        public void PlayerFold()
        {
            SendBroadcastPackage(ServerResponse.PlayerFolded, _round.CurrentPlayerIndex);
            _round.PlayerFolded();
        }

        public void PlayerRaise(int raisedToAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerRaised, _round.CurrentPlayerIndex, raisedToAmount);
            _round.PlayerRaised(raisedToAmount);
        }

        public void PlayerAllIn(int allInAmount)
        {
            SendBroadcastPackage(ServerResponse.PlayerAllIn, _round.CurrentPlayerIndex, allInAmount);
            _round.PlayerAllIn(allInAmount);
        }

        public void PlayerSendChatMessage(int index, string message)
        {
            SendBroadcastPackage(ServerResponse.ChatMessage, index, message);
        }

        private void SendBroadcastPackage(params object[] items) =>
            new Client.Package(Table.GetActiveClients()).Append(items, item => item).Send();

        //===========================================================
        //                      Template methods
        //===========================================================

        protected abstract void Kick(TablePlayer player);
        protected abstract void OnPlayerJoined();
        protected abstract void OnRoundFinished();
    }
}