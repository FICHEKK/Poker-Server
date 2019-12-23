using System;
using System.Collections.Generic;
using System.Threading;
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

            Round = new Round(Table.SmallBlind, Table.GetPlayerArray(), bigBlindIndex);
            Round.RoundPhaseChanged += RoundPhaseChangedEventHandler;
            Round.CurrentPlayerChanged += CurrentPlayerChangedEventHandler;
            Round.Start();
        }

        #region Event handlers

        private void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e) {
            Broadcast(ServerResponse.PlayerJoined);
            Broadcast(e.Username);
            Broadcast(e.Stack.ToString());

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
            switch (e.CurrentPhase) {
                case Round.Phase.PreFlop:
                    Broadcast(ServerResponse.Hand);
                    DealHandCards();
                    break;
                
                case Round.Phase.Flop:
                    Broadcast(ServerResponse.Flop);
                    RevealCommunityCards(3);
                    break;
                
                case Round.Phase.Turn:
                    Broadcast(ServerResponse.Turn);
                    RevealCommunityCards(1);
                    break;
                
                case Round.Phase.River:
                    Broadcast(ServerResponse.River);
                    RevealCommunityCards(1);
                    break;
                
                case Round.Phase.Showdown:
                    List<int> winnerIndexes = DetermineWinnerIndexes();
                    
                    Broadcast(ServerResponse.Showdown);
                    Broadcast(winnerIndexes.Count.ToString());
                    
                    foreach (int index in winnerIndexes) {
                        Broadcast(index.ToString());
                    }
                    
                    Thread.Sleep(3000);
                    Broadcast(ServerResponse.RoundFinished);
                    StartNewRound();
                    break;
                
                case Round.Phase.OnePlayerLeft:
                    Broadcast(ServerResponse.Showdown);
                    Broadcast(1.ToString());
                    Broadcast(Round.GetActivePlayers()[0].Index.ToString());
                    
                    Thread.Sleep(3000);
                    Broadcast(ServerResponse.RoundFinished);
                    StartNewRound();
                    break;
            }
        }

        private void DealHandCards() {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                if (!Table.IsSeatOccupied(i)) continue;
                
                Signal(i, Deck.GetNextCard().ToString());
                Signal(i, Deck.GetNextCard().ToString());
            }
        }

        private void RevealCommunityCards(int count) {
            for (int i = 0; i < count; i++) {
                Card card = Deck.GetNextCard();
                Broadcast(card.ToString());
                Round.AddCommunityCard(card);
            }
        }

        private List<int> DetermineWinnerIndexes() {
            List<Card> cards = Round.CommunityCards;
            List<int> winnerIndexes = new List<int>();

            Hand bestHand = null;

            foreach (TablePlayer player in Round.GetActivePlayers()) {
                Card handCard1 = player.GetFirstHandCard();
                Card handCard2 = player.GetSecondHandCard();
                
                SevenCardEvaluator evaluator = new SevenCardEvaluator(handCard1, handCard2,
                    cards[0], cards[1], cards[2], cards[3], cards[4]);
                
                if (bestHand == null) {
                    bestHand = evaluator.BestHand;
                    winnerIndexes.Add(player.Index);
                    continue;
                }

                int result = bestHand.CompareTo(evaluator.BestHand);

                if (result < 0) {
                    winnerIndexes.Clear();
                    winnerIndexes.Add(player.Index);
                    bestHand = evaluator.BestHand;
                }
                else if (result == 0) {
                    winnerIndexes.Add(player.Index);
                }
            }

            return winnerIndexes;
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