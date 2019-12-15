using System;
using Poker.EventArguments;
using Poker.Players;

namespace Poker {
    
    /// <summary>
    /// Encapsulates data of a single poker table.
    /// </summary>
    public class Table {

        /// <summary>Event that gets raised every time a player joins this table.</summary>
        public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

        /// <summary>Event that gets raised every time a player leaves this table.</summary>
        public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

        /// <summary>This table's current phase.</summary>
        public TablePhase Phase { get; set; } = TablePhase.Waiting;
        
        /// <summary>This table's small blind.</summary>
        public int SmallBlind { get; }
        
        /// <summary>This table's big blind.</summary>
        public int BigBlind => SmallBlind * 2;
        
        /// <summary>Minimum amount of chips needed to join this table.</summary>
        public int MinimumBuyIn => BigBlind * 10;

        /// <summary>Current dealer button index.</summary>
        public int ButtonIndex => _buttonIndex;
        
        /// <summary>Current number of players at the table.</summary>
        public int PlayerCount => _playerCount;
        
        /// <summary>Maximum number of players this table can seat.</summary>
        public int MaxPlayers { get; }
        
        /// <summary>True if table currently has 0 players.</summary>
        public bool IsEmpty => PlayerCount == 0;
        
        /// <summary>True if there are more than 0 and less than maximum number of players.</summary>
        public bool IsNotEmptyNorFull => !IsEmpty && !IsFull;
        
        /// <summary>True if there are maximum number of players at the table.</summary>
        public bool IsFull => PlayerCount == MaxPlayers;

        /// <summary>This table's dealer.</summary>
        public Dealer Dealer { get; }

        private readonly Seat[] _seats;
        private int _buttonIndex;
        private int _playerCount;

        /// <summary>
        /// Constructs a new table with the given small blind and maximum number of players.
        /// </summary>
        /// <param name="smallBlind">The small blind.</param>
        /// <param name="maxPlayers">Maximum number of players.</param>
        public Table(int smallBlind, int maxPlayers) {
            SmallBlind = smallBlind;
            MaxPlayers = maxPlayers;
            Dealer = new Dealer(this);
            _seats = new Seat[MaxPlayers];

            for (int i = 0; i < MaxPlayers; i++) {
                _seats[i] = new Seat(i);
            }
        }
        
        /// <summary>
        /// Increments the button position, skipping all the empty seats on the way.
        /// </summary>
        public void IncrementButtonIndex() {
            for (int i = 1; i <= MaxPlayers; i++) {
                _buttonIndex++;
                _buttonIndex %= MaxPlayers;

                if(_seats[_buttonIndex].IsOccupied) break;
            }
        }

        /// <summary>
        /// Checks if the seat on the given position is empty.
        /// </summary>
        /// <param name="index">The seat's index.</param>
        /// <returns>True if the seat at the given position is empty, false otherwise.</returns>
        public bool IsSeatEmpty(int index) {
            ValidateIndexRange(index);
            return _seats[index].IsEmpty;
        }

        /// <summary>
        /// Finds and returns the index of the first free seat, if there is one.
        /// </summary>
        /// <returns>Index of the first free seat if found, -1 if there are no free seats.</returns>
        public int GetFirstFreeSeatIndex() {
            for (int i = 0; i < MaxPlayers; i++) {
                if (_seats[i].IsEmpty) return i;
            }

            return -1;
        }

        public Seat GetSeatAt(int index) {
            ValidateIndexRange(index);
            return _seats[index];
        }

        public int GetIndexOf(string username) {
            for (int i = 0; i < MaxPlayers; i++) {
                if(_seats[i].IsEmpty) continue;
                if(_seats[i].Player.Username != username) continue;

                return i;
            }

            return -1;
        }

        /// <summary>
        /// Adds the given player to the first empty seat, if there is any.
        /// </summary>
        /// <param name="player">Player to be added to the table.</param>
        /// <param name="chipCount">The amount of chips the player is buying-in with.</param>
        public bool AddPlayer(TablePlayer player, int chipCount) {
            int index = GetFirstFreeSeatIndex();
            if (index < 0) return false;

            _seats[index].Player = player ?? throw new ArgumentNullException(nameof(player));
            _seats[index].ChipCount = chipCount;
            _playerCount++;

            OnPlayerJoined(new PlayerJoinedEventArgs(index, player.Username, chipCount));
            return true;
        }

        public void RemovePlayer(TablePlayer player) {
            for (int i = 0; i < MaxPlayers; i++) {
                if(_seats[i].IsEmpty || !_seats[i].Player.Equals(player)) continue;

                _seats[i].Player = null;
                _playerCount--;

                OnPlayerLeft(new PlayerLeftEventArgs(i));
                break;
            }
        }

        private void ValidateIndexRange(int index) {
            if (index < 0 || index >= MaxPlayers)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        #region Events

        protected virtual void OnPlayerJoined(PlayerJoinedEventArgs args) => PlayerJoined?.Invoke(this, args);
        protected virtual void OnPlayerLeft(PlayerLeftEventArgs args) => PlayerLeft?.Invoke(this, args);
        
        #endregion

        #region Signal and broadcast

        /// <summary>
        /// Sends a server response to the single specified client.
        /// </summary>
        /// <param name="username">Username of the client that is the receiver.</param>
        /// <param name="response">The response to be sent.</param>
        public void Signal(string username, ServerResponse response) {
            foreach (Seat seat in _seats) {
                if(seat.IsEmpty || seat.Player.Username != username) continue;

                seat.Player.Writer.BaseStream.WriteByte((byte) response);
                break;
            }
        }
    
        /// <summary>
        /// Sends given data to the single specified client.
        /// </summary>
        /// <param name="username">Username of the client that is the receiver.</param>
        /// <param name="data">The data to be sent.</param>
        public void Signal(string username, string data) {
            foreach (Seat seat in _seats) {
                if(seat.IsEmpty || seat.Player.Username != username) continue;

                seat.Player.Writer.WriteLine(data);
                break;
            }
        }
        
        /// <summary>
        /// Sends a server response to the single specified position.
        /// </summary>
        /// <param name="index">The index of the player to send a response to.</param>
        /// <param name="response">The response to be sent.</param>
        public void Signal(int index, ServerResponse response) {
            ValidateIndexRange(index);
            _seats[index].Player?.Writer.BaseStream.WriteByte((byte) response);
        }
    
        /// <summary>
        /// Sends given data to the single specified position.
        /// </summary>
        /// <param name="index">The index of the client to send a response to.</param>
        /// <param name="data">The data to be sent.</param>
        public void Signal(int index, string data) {
            ValidateIndexRange(index);
            _seats[index].Player?.Writer.WriteLine(data);
        }
    
        /// <summary>
        /// Sends a server response to every player on the table.
        /// </summary>
        /// <param name="response">The response to be sent.</param>
        public void Broadcast(ServerResponse response) {
            foreach (Seat seat in _seats) {
                seat.Player?.Writer.BaseStream.WriteByte((byte) response);
            }
        }

        /// <summary>
        /// Sends given data to every player on the table.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public void Broadcast(string data) {
            foreach (Seat seat in _seats) {
                seat.Player?.Writer.WriteLine(data);
            }
        }
        
        #endregion
    }
}