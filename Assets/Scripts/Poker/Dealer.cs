using System;
using System.Collections.Generic;
using System.Threading;
using Dao;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;
using Timer = System.Timers.Timer;

namespace Poker
{
    /// <summary>Models a poker table dealer.</summary>
    public class Dealer
    {
        /// <summary>The table that this dealer is dealing on.</summary>
        public Table Table { get; }

        /// <summary>The current state of the round.</summary>
        public Round Round { get; private set; }

        /// <summary>The deck used by this dealer to deal cards.</summary>
        public Deck Deck { get; } = new Deck();

        /// <summary> Timer used to time player's turn durations. </summary>
        private readonly Timer _decisionTimer = new Timer {AutoReset = false};

        public Dealer(Table table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Table.PlayerJoined += PlayerJoinedEventHandler;
            Table.PlayerLeft += PlayerLeftEventHandler;
            
            _decisionTimer.Interval = (TableConstant.PlayerTurnDuration + TableConstant.PlayerTurnOvertime) * 1000;
            _decisionTimer.Elapsed += (sender, e) =>
            {
                if (Round == null) return;
                if (Round.CurrentPhase == Round.Phase.OnePlayerLeft) return;
                if (Round.CurrentPhase == Round.Phase.Showdown) return;
                
                Broadcast(ServerResponse.PlayerFolded);
                Broadcast(Round.PlayerIndex);
                Round.PlayerFolded();
            };
        }
        
        //----------------------------------------------------------------
        //                     Starting new round
        //----------------------------------------------------------------

        private void StartNewRound()
        {
            Deck.Shuffle();
            Table.IncrementButtonIndex();

            int smallBlindIndex = Table.GetNextOccupiedSeatIndex(Table.DealerButtonIndex);
            int bigBlindIndex = Table.GetNextOccupiedSeatIndex(smallBlindIndex);
            int playerIndex = Table.GetNextOccupiedSeatIndex(bigBlindIndex);

            Round = new Round(Table.GetPlayerArray(), smallBlindIndex, bigBlindIndex, playerIndex, Table.SmallBlind);
            Round.RoundPhaseChanged += RoundPhaseChangedEventHandler;
            Round.CurrentPlayerChanged += CurrentPlayerChangedEventHandler;
            
            BroadcastBlindsData(smallBlindIndex, bigBlindIndex);
            DealHandCards();
            Round.Start();
        }

        private void BroadcastBlindsData(int smallBlindIndex, int bigBlindIndex)
        {
            Broadcast(ServerResponse.Blinds);
            
            Broadcast(Round.JustJoinedPlayerIndexes.Count);
            foreach (var index in Round.JustJoinedPlayerIndexes) Broadcast(index);

            Broadcast(Table.DealerButtonIndex);
            Broadcast(smallBlindIndex);
            Broadcast(bigBlindIndex);
        }

        private void DealHandCards()
        {
            Broadcast(ServerResponse.Hand);

            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                if (!Table.IsSeatOccupied(i)) continue;

                Card handCard1 = Deck.GetNextCard();
                Card handCard2 = Deck.GetNextCard();

                Table.GetPlayerAt(i).SetHand(handCard1, handCard2);

                Signal(i, handCard1);
                Signal(i, handCard2);
            }
        }
        
        //----------------------------------------------------------------
        //                    Flop, turn & river
        //----------------------------------------------------------------

        private void RevealCommunityCards(ServerResponse response, int cardCount)
        {
            Broadcast(response);
            
            for (int i = 0; i < cardCount; i++)
            {
                Card card = Deck.GetNextCard();
                Broadcast(card);
                Round.AddCommunityCard(card);
            }
            
            Thread.Sleep(TableConstant.PausePerCardDuration * cardCount);
        }

        private void ProcessShowdown()
        {
            RevealActivePlayersCards();

            var sidePots = Pot.CalculateSidePots(Round.ParticipatingPlayers);
            var winningPlayers = new HashSet<TablePlayer>();
            
            Broadcast(ServerResponse.Showdown);
            Broadcast(sidePots.Count);

            for (int i = sidePots.Count - 1; i >= 0; i--)
            {
                var sidePotWinners = DetermineWinners(sidePots[i].Contenders, Round.CommunityCards, out var bestHandValue);
                var winAmount = sidePots[i].Value / sidePotWinners.Count;
                
                foreach (var player in sidePotWinners)
                {
                    player.Stack += winAmount;
                    player.ChipCount += winAmount;
                    winningPlayers.Add(player);
                }

                BroadcastSidePotData(sidePots[i].Value, sidePotWinners.Count, sidePotWinners, bestHandValue);
            }
            
            foreach (var player in winningPlayers)
                DaoProvider.Dao.SetWinCount(player.Username, DaoProvider.Dao.GetWinCount(player.Username) + 1);

            foreach (var participant in Round.ParticipatingPlayers)
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            
            Thread.Sleep(TableConstant.RoundFinishPauseDuration + sidePots.Count * 1000);
            KickBrokePlayers();
            Broadcast(ServerResponse.RoundFinished);

            if (Table.PlayerCount >= 2) StartNewRound();
        }
        
        private void KickBrokePlayers()
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
            {
                var player = Table.GetPlayerAt(i);
                if(player == null) continue;

                if (player.Stack == 0)
                {
                    Signal(i, ServerResponse.LeaveTableSuccess);
                    
                    Casino.RemoveTablePlayer(player);
                    Casino.AddLobbyPlayer(new LobbyPlayer(player.Username, player.ChipCount, player.Reader, player.Writer));
                }
            }
        }

        private void BroadcastSidePotData(int value, int winnerCount, List<TablePlayer> winners, string bestHandValue)
        {
            Broadcast(value);
            Broadcast(winnerCount);
            foreach (var winner in winners) Broadcast(winner.Index);
            Broadcast(bestHandValue);
        }

        private void RevealActivePlayersCards()
        {
            Broadcast(ServerResponse.CardsReveal);
            Broadcast(Round.ActivePlayers.Count);
            
            foreach (var player in Round.ActivePlayers)
            {
                Broadcast(player.Index);
                Broadcast(player.FirstHandCard);
                Broadcast(player.SecondHandCard);
            }
        }

        private static List<TablePlayer> DetermineWinners(List<TablePlayer> players, List<Card> communityCards, out string bestHandValue)
        {
            if(players == null) throw new ArgumentNullException(nameof(players));
            if(communityCards == null) throw new ArgumentNullException(nameof(communityCards));
            if(players.Count == 0) throw new ArgumentException("Player collection must be non-empty.");
            if(communityCards.Count != 5) throw new ArgumentException("Expected all 5 community cards.");
            
            var winners = new List<TablePlayer>();

            Hand bestHand = null;

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

                int result = bestHand.CompareTo(evaluator.BestHand);

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

            bestHandValue = bestHand.ToStringPretty();
            
            return winners;
        }

        private void ProcessOnePlayerLeft()
        {
            var winner = Round.ActivePlayers[0];
            
            Broadcast(ServerResponse.Showdown);
            Broadcast(1);
            Broadcast(Round.Pot);
            Broadcast(1);
            Broadcast(winner.Index);
            Broadcast(string.Empty);
            
            DaoProvider.Dao.SetWinCount(winner.Username, DaoProvider.Dao.GetWinCount(winner.Username) + 1);
            winner.Stack += Round.Pot;
            winner.ChipCount += Round.Pot;
            
            foreach (var participant in Round.ParticipatingPlayers)
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            
            Thread.Sleep(TableConstant.RoundFinishPauseDuration);
            KickBrokePlayers();
            Broadcast(ServerResponse.RoundFinished);

            if (Table.PlayerCount >= 2) StartNewRound();
        }

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
            Broadcast(ServerResponse.PlayerIndex);
            Broadcast(e.CurrentPlayerIndex);
            
            Signal(e.CurrentPlayerIndex, ServerResponse.RequiredBet);
            Signal(e.CurrentPlayerIndex, e.RequiredCall);
            Signal(e.CurrentPlayerIndex, e.MinRaise);
            Signal(e.CurrentPlayerIndex, e.MaxRaise);
            
            _decisionTimer.Stop();
            _decisionTimer.Start();
        }

        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e)
        {
            Broadcast(ServerResponse.PlayerJoined);
            Broadcast(e.Player.Index);
            Broadcast(e.Player.Username);
            Broadcast(e.Player.Stack);

            if (Table.PlayerCount == 2) StartNewRound();
        }

        private void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e)
        {
            Broadcast(ServerResponse.PlayerLeft);
            Broadcast(e.Index);

            Round?.PlayerLeft(e.Index);

            if (Table.PlayerCount == 1)
            {
                Round = null;
            }
        }

        /// <summary> Sends a server response to the single specified position. </summary>
        /// <param name="index"> The index of the player to send a response to. </param>
        /// <param name="response"> The response to be sent. </param>
        private void Signal(int index, ServerResponse response) =>
            Table.GetPlayerAt(index)?.Writer.BaseStream.WriteByte((byte) response);

        /// <summary> Sends given data to the single specified position. </summary>
        /// <param name="index"> The index of the client to send a response to. </param>
        /// <param name="data"> The data to be sent. </param>
        private void Signal<T>(int index, T data) =>
            Table.GetPlayerAt(index)?.Writer.WriteLine(data.ToString());

        /// <summary> Sends a server response to every player on the table. </summary>
        /// <param name="response"> The response to be sent. </param>
        public void Broadcast(ServerResponse response)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
                Signal(i, response);
        }

        /// <summary> Sends given data to every player on the table. </summary>
        /// <param name="data"> The data to be sent. </param>
        public void Broadcast<T>(T data)
        {
            for (int i = 0; i < Table.MaxPlayers; i++)
                Signal(i, data);
        }
    }
}