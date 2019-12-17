using System.Threading;
using Poker.Cards;
using Poker.EventArguments;

namespace Poker {
    
    /// <summary>
    /// Models a poker table dealer.
    /// </summary>
    public class Dealer {
        
        /// <summary>The table that this dealer is dealing on.</summary>
        public Table Table { get; }
        
        /// <summary>The current state of the round.</summary>
        public Round Round { get; private set; }
        
        /// <summary>The deck used by this dealer to deal cards.</summary>
        public Deck Deck { get; } = new Deck();

        /// <summary>
        /// Constructs a new dealer on the specified table.
        /// </summary>
        /// <param name="table">The table to be dealt on.</param>
        public Dealer(Table table) {
            Table = table;
            Table.PlayerJoined += PlayerJoinedEventHandler;
            Table.PlayerLeft += PlayerLeftEventHandler;
        }
        
        private void StartNewRound() {
            int smallBlindIndex = Table.GetNextOccupiedSeatIndex(Table.ButtonIndex);
            int bigBlindIndex = Table.GetNextOccupiedSeatIndex(smallBlindIndex);
            
            Broadcast(ServerResponse.Blinds);
            Broadcast(smallBlindIndex.ToString());
            Broadcast(bigBlindIndex.ToString());

            Round = new Round(Table.SmallBlind, Table.GetSeatsArray(), bigBlindIndex);
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
            if (e.CurrentPhase == Round.Phase.PreFlop) {
                Deck.Shuffle();
                DealHandCards();
            }
            else if (e.CurrentPhase == Round.Phase.Flop) {
                Broadcast(ServerResponse.Flop);
                Broadcast(Deck.GetNextCard().ToString());
                Broadcast(Deck.GetNextCard().ToString());
                Broadcast(Deck.GetNextCard().ToString());
            }
            else if (e.CurrentPhase == Round.Phase.Turn) {
                Broadcast(ServerResponse.Turn);
                Broadcast(Deck.GetNextCard().ToString());
            }
            else if (e.CurrentPhase == Round.Phase.River) {
                Broadcast(ServerResponse.River);
                Broadcast(Deck.GetNextCard().ToString());
            }
            else if (e.CurrentPhase == Round.Phase.Showdown) {
                DetermineWinner();

                Thread.Sleep(3000);

                Broadcast(ServerResponse.RoundFinished);
                Table.IncrementButtonIndex();
                
                StartNewRound();
            }
        }

        private void DealHandCards() {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                if(!Table.GetSeatAt(i).IsOccupied) continue;
                
                Signal(i, ServerResponse.Hand);
                Signal(i, Deck.GetNextCard().ToString());
                Signal(i, Deck.GetNextCard().ToString());
            }
        }

        private void DetermineWinner() {
            Broadcast(ServerResponse.Showdown);
            // TODO get all active players and find the best combination
            // send winner flag to everyone with pot size winning
            // in case of multiple winners, we need to send winner count also
        }

        private void CurrentPlayerChangedEventHandler(object sender, CurrentPlayerChangedEventArgs e) {
            Broadcast(ServerResponse.PlayerIndex);
            Broadcast(e.CurrentPlayerIndex.ToString());
            
            Signal(e.CurrentPlayerIndex, ServerResponse.RequiredBet);
            Signal(e.CurrentPlayerIndex, Round.CurrentHighestBet.ToString());
        }

        #endregion
        
        #region Signal and broadcast

        /// <summary>
        /// Sends a server response to the single specified client.
        /// </summary>
        /// <param name="username">Username of the client that is the receiver.</param>
        /// <param name="response">The response to be sent.</param>
        public void Signal(string username, ServerResponse response) {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                Seat seat = Table.GetSeatAt(i);

                if (seat.IsOccupied && seat.Player.Username == username) {
                    seat.Player.Writer.BaseStream.WriteByte((byte) response);
                    break;
                }
            }
        }
    
        /// <summary>
        /// Sends given data to the single specified client.
        /// </summary>
        /// <param name="username">Username of the client that is the receiver.</param>
        /// <param name="data">The data to be sent.</param>
        public void Signal(string username, string data) {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                Seat seat = Table.GetSeatAt(i);

                if (seat.IsOccupied && seat.Player.Username == username) {
                    seat.Player.Writer.WriteLine(data);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Sends a server response to the single specified position.
        /// </summary>
        /// <param name="index">The index of the player to send a response to.</param>
        /// <param name="response">The response to be sent.</param>
        public void Signal(int index, ServerResponse response) {
            Seat seat = Table.GetSeatAt(index);
            
            if (seat.IsOccupied) {
                seat.Player.Writer.BaseStream.WriteByte((byte) response);
            }
        }
    
        /// <summary>
        /// Sends given data to the single specified position.
        /// </summary>
        /// <param name="index">The index of the client to send a response to.</param>
        /// <param name="data">The data to be sent.</param>
        public void Signal(int index, string data) {
            Seat seat = Table.GetSeatAt(index);
            
            if (seat.IsOccupied) {
                seat.Player.Writer.WriteLine(data);
            }
        }
    
        /// <summary>
        /// Sends a server response to every player on the table.
        /// </summary>
        /// <param name="response">The response to be sent.</param>
        public void Broadcast(ServerResponse response) {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                Seat seat = Table.GetSeatAt(i);

                if (seat.IsOccupied) {
                    seat.Player.Writer.BaseStream.WriteByte((byte) response);
                }
            }
        }

        /// <summary>
        /// Sends given data to every player on the table.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public void Broadcast(string data) {
            for (int i = 0; i < Table.MaxPlayers; i++) {
                Seat seat = Table.GetSeatAt(i);

                if (seat.IsOccupied) {
                    seat.Player.Writer.WriteLine(data);
                }
            }
        }
        
        #endregion
    }
}
