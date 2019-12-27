using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dao;
using Poker.Cards;
using Poker.EventArguments;
using Poker.Players;

namespace Poker {
    
    /// <summary>Models a poker table dealer.</summary>
    public class Dealer {

        /// <summary>The table that this dealer is dealing on.</summary>
        public Table Table { get; }

        /// <summary>The current state of the round.</summary>
        public Round Round { get; private set; }

        /// <summary>The deck used by this dealer to deal cards.</summary>
        public Deck Deck { get; } = new Deck();

        public Dealer(Table table) {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Table.PlayerJoined += PlayerJoinedEventHandler;
            Table.PlayerLeft += PlayerLeftEventHandler;
        }

        private void StartNewRound() {
            Deck.Shuffle();
            Table.IncrementButtonIndex();
            
            int smallBlindIndex = Table.GetNextOccupiedSeatIndex(Table.ButtonIndex);
            int bigBlindIndex = Table.GetNextOccupiedSeatIndex(smallBlindIndex);

            Broadcast(ServerResponse.Blinds);
            Broadcast(smallBlindIndex.ToString());
            Broadcast(bigBlindIndex.ToString());

            Round = new Round(Table.GetPlayerArray(), bigBlindIndex);
            Round.ChipsPlaced(smallBlindIndex, Table.SmallBlind);
            Round.ChipsPlaced(bigBlindIndex, Table.BigBlind);
            Round.RoundPhaseChanged += RoundPhaseChangedEventHandler;
            Round.CurrentPlayerChanged += CurrentPlayerChangedEventHandler;
            Round.Start();
        }

        #region Event handlers

        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e) {
            Broadcast(ServerResponse.PlayerJoined);
            Broadcast(e.Player.Username);
            Broadcast(e.Player.Stack.ToString());

            if (Table.PlayerCount == 2) {
                StartNewRound();
            }
        }

        private void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e) {
            Broadcast(ServerResponse.PlayerLeft);
            Broadcast(e.Index.ToString());

            // TODO remove player from round and if there is only 1 player left, he wins the round
        }

        private void RoundPhaseChangedEventHandler(object sender, RoundPhaseChangedEventArgs e) {
            if (e.CurrentPhase == Round.Phase.PreFlop) {
                ProcessPreFlop();
            }
            else if (e.CurrentPhase == Round.Phase.Flop) {
                ProcessFlop();
            }
            else if (e.CurrentPhase == Round.Phase.Turn) {
                ProcessTurn();
            }
            else if (e.CurrentPhase == Round.Phase.River) {
                ProcessRiver();
            }
            else if (e.CurrentPhase == Round.Phase.Showdown) {
                ProcessShowdown();
            }
            else if (e.CurrentPhase == Round.Phase.OnePlayerLeft) {
                ProcessOnePlayerLeft();
            }
        }

        private void ProcessPreFlop() {
            Broadcast(ServerResponse.Hand);
            
            for (int i = 0; i < Table.MaxPlayers; i++) {
                if (!Table.IsSeatOccupied(i)) continue;

                Card handCard1 = Deck.GetNextCard();
                Card handCard2 = Deck.GetNextCard();
                
                Table.GetPlayerAt(i).SetHand(handCard1, handCard2);
                
                Signal(i, handCard1.ToString());
                Signal(i, handCard2.ToString());
            }
        }

        private void ProcessFlop() {
            Broadcast(ServerResponse.Flop);
            RevealCommunityCard();
            RevealCommunityCard();
            RevealCommunityCard();
        }

        private void ProcessTurn() {
            Broadcast(ServerResponse.Turn);
            RevealCommunityCard();
        }

        private void ProcessRiver() {
            Broadcast(ServerResponse.River);
            RevealCommunityCard();
        }
        
        private void RevealCommunityCard() {
            Card card = Deck.GetNextCard();
            Broadcast(card.ToString());
            Round.AddCommunityCard(card);
        }

        private void ProcessShowdown() {
            FinishThisRound(DetermineWinners());
        }

        private void ProcessOnePlayerLeft() {
            FinishThisRound(new List<TablePlayer> { Round.GetActivePlayers()[0] });
        }

        private void FinishThisRound(List<TablePlayer> winners) {
            int winAmount = Round.CurrentPot / winners.Count;
            
            foreach (TablePlayer winner in winners) {
                DaoProvider.Dao.SetWinCount(winner.Username, DaoProvider.Dao.GetWinCount(winner.Username) + 1);
                winner.Stack += winAmount;
                winner.ChipCount += winAmount;
            }
            
            foreach (TablePlayer participant in Round.GetParticipatingPlayers()) {
                DaoProvider.Dao.SetChipCount(participant.Username, participant.ChipCount);
            }

            Broadcast(ServerResponse.Showdown);
            Broadcast(winners.Count.ToString());

            foreach (TablePlayer winner in winners) {
                Broadcast(winner.Index.ToString());
            }

            Thread.Sleep(2000);
            Broadcast(ServerResponse.RoundFinished);
            StartNewRound();
        }

        private List<TablePlayer> DetermineWinners() {
            List<Card> cards = Round.CommunityCards;
            List<TablePlayer> winners = new List<TablePlayer>();

            Hand bestHand = null;

            foreach (TablePlayer player in Round.GetActivePlayers()) {
                Card handCard1 = player.GetFirstHandCard();
                Card handCard2 = player.GetSecondHandCard();
                
                SevenCardEvaluator evaluator = new SevenCardEvaluator(handCard1, handCard2,
                    cards[0], cards[1], cards[2], cards[3], cards[4]);
                
                if (bestHand == null) {
                    bestHand = evaluator.BestHand;
                    winners.Add(player);
                    continue;
                }

                int result = bestHand.CompareTo(evaluator.BestHand);

                if (result < 0) {
                    winners.Clear();
                    winners.Add(player);
                    bestHand = evaluator.BestHand;
                }
                else if (result == 0) {
                    winners.Add(player);
                }
            }

            return winners;
        }

        private void CurrentPlayerChangedEventHandler(object sender, CurrentPlayerChangedEventArgs e) {
            Broadcast(ServerResponse.PlayerIndex);
            Broadcast(e.CurrentPlayerIndex.ToString());

            Signal(e.CurrentPlayerIndex, ServerResponse.RequiredBet);
            Signal(e.CurrentPlayerIndex, Round.CurrentHighestBet.ToString());
        }

        #endregion

        #region Signal and broadcast

        /// <summary> Sends a server response to the single specified client. </summary>
        /// <param name="username"> Username of the client that is the receiver. </param>
        /// <param name="response"> The response to be sent. </param>
        public void Signal(string username, ServerResponse response) {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                TablePlayer player = Table.GetPlayerAt(i);

                if (player != null && player.Username == username) {
                    player.Writer.BaseStream.WriteByte((byte) response);
                    break;
                }
            }
        }

        /// <summary> Sends given data to the single specified client. </summary>
        /// <param name="username"> Username of the client that is the receiver. </param>
        /// <param name="data"> The data to be sent. </param>
        public void Signal(string username, string data) {
            for (var i = 0; i < Table.MaxPlayers; i++) {
                var player = Table.GetPlayerAt(i);

                if (player != null && player.Username == username) {
                    player.Writer.WriteLine(data);
                    break;
                }
            }
        }

        /// <summary> Sends a server response to the single specified position. </summary>
        /// <param name="index"> The index of the player to send a response to. </param>
        /// <param name="response"> The response to be sent. </param>
        public void Signal(int index, ServerResponse response) {
            Table.GetPlayerAt(index)?.Writer.BaseStream.WriteByte((byte) response);
        }

        /// <summary> Sends given data to the single specified position. </summary>
        /// <param name="index"> The index of the client to send a response to. </param>
        /// <param name="data"> The data to be sent. </param>
        public void Signal(int index, string data) {
            Table.GetPlayerAt(index)?.Writer.WriteLine(data);
        }

        /// <summary> Sends a server response to every player on the table. </summary>
        /// <param name="response"> The response to be sent. </param>
        public void Broadcast(ServerResponse response) {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                Table.GetPlayerAt(i)?.Writer.BaseStream.WriteByte((byte) response);
            }
        }

        /// <summary> Sends given data to every player on the table. </summary>
        /// <param name="data"> The data to be sent. </param>
        public void Broadcast(string data) {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                Table.GetPlayerAt(i)?.Writer.WriteLine(data);
            }
        }

        #endregion
    }
}