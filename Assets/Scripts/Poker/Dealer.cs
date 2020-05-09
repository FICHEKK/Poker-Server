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

                var package = new Client.Package(Table.GetActiveClients());
                package.Append(ServerResponse.PlayerFolded);
                package.Append(Round.PlayerIndex);
                package.Send();
                
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
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.Blinds);
            package.Append(Round.JustJoinedPlayerIndexes.Count);
            Round.JustJoinedPlayerIndexes.ForEach(index => package.Append(index));
            package.Append(Table.DealerButtonIndex);
            package.Append(smallBlindIndex);
            package.Append(bigBlindIndex);
            package.Send();
        }

        private void DealHandCards()
        {
            foreach (var player in Table)
            {
                Card handCard1 = Deck.GetNextCard();
                Card handCard2 = Deck.GetNextCard();

                player.SetHand(handCard1, handCard2);

                var package = new Client.Package(player.Client);
                package.Append(ServerResponse.Hand);
                package.Append(handCard1);
                package.Append(handCard2);
                package.Send();
            }
        }
        
        //----------------------------------------------------------------
        //                    Flop, turn & river
        //----------------------------------------------------------------

        private void RevealCommunityCards(ServerResponse response, int cardCount)
        {
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(response);

            for (int i = 0; i < cardCount; i++)
            {
                Card card = Deck.GetNextCard();
                package.Append(card);
                Round.AddCommunityCard(card);
            }
            
            package.Send();
            Thread.Sleep(TableConstant.PausePerCardDuration * cardCount);
        }

        private void ProcessShowdown()
        {
            RevealActivePlayersCards();

            var sidePots = Pot.CalculateSidePots(Round.ParticipatingPlayers);
            var winningPlayers = new HashSet<TablePlayer>();

            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.Showdown);
            package.Append(sidePots.Count);

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
                
                package.Append(sidePots[i].Value);
                package.Append(sidePotWinners.Count);
                sidePotWinners.ForEach(winner => package.Append(winner.Index));
                package.Append(bestHandValue);
            }
            
            package.Send();
            
            FinishRound(TableConstant.RoundFinishPauseDuration + sidePots.Count * 1000, winningPlayers);
        }

        private void RevealActivePlayersCards()
        {
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.CardsReveal);
            package.Append(Round.ActivePlayers.Count);
            
            foreach (var player in Round.ActivePlayers)
            {
                package.Append(player.Index);
                package.Append(player.FirstHandCard);
                package.Append(player.SecondHandCard);
            }
            
            package.Send();
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

            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.Showdown);
            package.Append(1);
            package.Append(Round.Pot);
            package.Append(1);
            package.Append(winner.Index);
            package.Append(string.Empty);
            package.Send();
            
            winner.Stack += Round.Pot;
            winner.ChipCount += Round.Pot;
            
            FinishRound(TableConstant.RoundFinishPauseDuration, new HashSet<TablePlayer>{winner});
        }

        private void FinishRound(int timeout, HashSet<TablePlayer> winningPlayers)
        {
            foreach (var player in winningPlayers)
                DaoProvider.Dao.SetWinCount(player.Username, DaoProvider.Dao.GetWinCount(player.Username) + 1);

            foreach (var participant in Round.ParticipatingPlayers)
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            
            Thread.Sleep(timeout);
            KickBrokePlayers();

            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.RoundFinished);
            package.Send();

            if (Table.PlayerCount >= 2) StartNewRound();
        }
        
        private void KickBrokePlayers()
        {
            foreach (var player in Table)
            {
                if (player.Stack > 0) continue;
                
                var package = new Client.Package(player.Client);
                package.Append(ServerResponse.LeaveTable);
                package.Append(ServerResponse.LeaveTableNoMoney);
                package.Send();
                
                Casino.RemoveTablePlayer(player);
                Casino.AddLobbyPlayer(new LobbyPlayer(player.Client, player.ChipCount));
            }
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
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.PlayerIndex);
            package.Append(e.CurrentPlayerIndex);
            package.Send();
            
            package = new Client.Package(Table[e.CurrentPlayerIndex].Client);
            package.Append(ServerResponse.RequiredBet);
            package.Append(e.RequiredCall);
            package.Append(e.MinRaise);
            package.Append(e.MaxRaise);
            package.Send();

            _decisionTimer.Stop();
            _decisionTimer.Start();
        }

        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e)
        {
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.PlayerJoined);
            package.Append(e.Player.Index);
            package.Append(e.Player.Username);
            package.Append(e.Player.Stack);
            package.Send();

            if (Table.PlayerCount == 2) StartNewRound();
        }

        private void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e)
        {
            var package = new Client.Package(Table.GetActiveClients());
            package.Append(ServerResponse.PlayerLeft);
            package.Append(e.Index);
            package.Send();

            Round?.PlayerLeft(e.Index);

            if (Table.PlayerCount == 1)
            {
                Round = null;
            }
        }
    }
}