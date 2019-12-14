using System;
using Poker.EventArguments;

namespace Poker {
    
    /// <summary>
    /// Encapsulates data of a single poker table.
    /// </summary>
    public class Table {

        /// <summary>Event that gets raised every time a player is added to this table.</summary>
        public event EventHandler<PlayerAddedEventArgs> PlayerAdded;

        /// <summary>Event that gets raised every time a player is removed from this table.</summary>
        public event EventHandler<PlayerRemovedEventArgs> PlayerRemoved;

        /// <summary>This table's current phase.</summary>
        public TablePhase Phase { get; set; } = TablePhase.Waiting;
        
        /// <summary>This table's small blind.</summary>
        public int SmallBlind { get; }
        
        /// <summary>This table's big blind.</summary>
        public int BigBlind => SmallBlind * 2;
        
        /// <summary>Minimum amount of chips needed to join this table.</summary>
        public int MinimumBuyIn => BigBlind * 10;

        /// <summary>Current dealer button position.</summary>
        public int ButtonPosition => _buttonPosition;
        
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

        private readonly Player[] _players;
        private int _buttonPosition;
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
            _players = new Player[MaxPlayers];
        }
        
        /// <summary>
        /// Increments the button position, skipping all the empty seats on the way.
        /// </summary>
        public void IncrementButtonPosition() {
            for (int i = 1; i <= MaxPlayers; i++) {
                _buttonPosition++;
                _buttonPosition %= MaxPlayers;

                if(_players[_buttonPosition] != null) break;
            }
        }

        /// <summary>
        /// Checks if the seat on the given position is empty.
        /// </summary>
        /// <param name="position">The seat's position.</param>
        /// <returns>True if the seat at the given position is empty, false otherwise.</returns>
        public bool IsSeatEmpty(int position) {
            ValidatePositionRange(position);
            return _players[position] == null;
        }

        /// <summary>
        /// Finds and returns the index of the first free seat, if there is one.
        /// </summary>
        /// <returns>Index of the first free seat if found, -1 if there are no free seats.</returns>
        public int GetFirstFreeSeat() {
            for (int i = 0; i < MaxPlayers; i++) {
                if (_players[i] == null) return i;
            }

            return -1;
        }
        
        public Player GetPlayerAt(int index) {
            ValidatePositionRange(index);
            return _players[index];
        }

        public void AddPlayer(Player player, int index) {
            ValidatePositionRange(index);
            
            if (_players[index] != null)
                throw new ArgumentException("Seat at the given position is already taken by another player.");

            _players[index] = player ?? throw new ArgumentNullException(nameof(player));
            _playerCount++;
            
            OnPlayerAdded(new PlayerAddedEventArgs(index, player.Username, player.ChipCount));
        }

        public void RemovePlayer(Player player) {
            for (int i = 0; i < MaxPlayers; i++) {
                if(_players[i] == null || !_players[i].Equals(player)) continue;

                _players[i] = null;
                _playerCount--;
                
                OnPlayerRemoved(new PlayerRemovedEventArgs(i));
                break;
            }
        }

        private void ValidatePositionRange(int position) {
            if (position < 0 || position >= MaxPlayers)
                throw new ArgumentOutOfRangeException(nameof(position));
        }

        #region Events

        protected virtual void OnPlayerAdded(PlayerAddedEventArgs args) => PlayerAdded?.Invoke(this, args);
        protected virtual void OnPlayerRemoved(PlayerRemovedEventArgs args) => PlayerRemoved?.Invoke(this, args);
        
        #endregion

        #region Signal and broadcast

        /// <summary>
        /// Sends a server response to the single specified client.
        /// </summary>
        /// <param name="username">Username of the client that is the receiver.</param>
        /// <param name="response">The response to be sent.</param>
        public void Signal(string username, ServerResponse response) {
            foreach (Player player in _players) {
                if (player == null || player.Username != username) continue;
            
                player.Writer.BaseStream.WriteByte((byte) response);
                break;
            }
        }
    
        /// <summary>
        /// Sends given data to the single specified client.
        /// </summary>
        /// <param name="username">Username of the client that is the receiver.</param>
        /// <param name="data">The data to be sent.</param>
        public void Signal(string username, string data) {
            foreach (Player player in _players) {
                if (player == null || player.Username != username) continue;
            
                player.Writer.WriteLine(data);
                break;
            }
        }
        
        /// <summary>
        /// Sends a server response to the single specified position.
        /// </summary>
        /// <param name="position">The position of the client to send a response to.</param>
        /// <param name="response">The response to be sent.</param>
        public void Signal(int position, ServerResponse response) {
            ValidatePositionRange(position);
            _players[position]?.Writer.BaseStream.WriteByte((byte) response);
        }
    
        /// <summary>
        /// Sends given data to the single specified position.
        /// </summary>
        /// <param name="position">The position of the client to send a response to.</param>
        /// <param name="data">The data to be sent.</param>
        public void Signal(int position, string data) {
            ValidatePositionRange(position);
            _players[position]?.Writer.WriteLine(data);
        }
    
        /// <summary>
        /// Sends a server response to every client on the table.
        /// </summary>
        /// <param name="response">The response to be sent.</param>
        public void Broadcast(ServerResponse response) {
            foreach (Player player in _players) {
                player?.Writer.BaseStream.WriteByte((byte) response);
            }
        }

        /// <summary>
        /// Sends given data to every client on the table.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        public void Broadcast(string data) {
            foreach (Player player in _players) {
                player?.Writer.WriteLine(data);
            }
        }
        
        #endregion
    }
}