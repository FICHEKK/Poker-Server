using System;
using Poker.EventArguments;
using Poker.Players;

namespace Poker
{
    /// <summary>
    /// Encapsulates data of a single poker table.
    /// </summary>
    public class Table
    {
        /// <summary>Event that gets raised every time a player joins this table.</summary>
        public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

        /// <summary>Event that gets raised every time a player leaves this table.</summary>
        public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

        /// <summary>This table's title (name).</summary>
        public string Title { get; }

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

        /// <summary>True if there are maximum number of players at the table.</summary>
        public bool IsFull => PlayerCount == MaxPlayers;

        /// <summary>This table's dealer.</summary>
        public Dealer Dealer { get; }

        private readonly TablePlayer[] _players;
        private int _buttonIndex;
        private int _playerCount;

        /// <summary> Constructs a new table with the given small blind and maximum number of players. </summary>
        /// <param name="title"> This table's title (name). </param>
        /// <param name="smallBlind"> The small blind. </param>
        /// <param name="maxPlayers"> Maximum number of players. </param>
        public Table(string title, int smallBlind, int maxPlayers)
        {
            Title = title;
            SmallBlind = smallBlind;
            MaxPlayers = maxPlayers;
            Dealer = new Dealer(this);
            _players = new TablePlayer[MaxPlayers];
        }

        #region Button

        /// <summary> Increments the button position, skipping all the empty seats on the way. </summary>
        public void IncrementButtonIndex()
        {
            _buttonIndex = GetNextOccupiedSeatIndex(_buttonIndex);

            if (_buttonIndex == -1)
            {
                _buttonIndex = 0;
            }
        }

        /// <summary> Finds and returns the next occupied seat index. </summary>
        /// <param name="start"> Starts searching from this index (start is not included). </param>
        /// <returns> Index of the next occupied seat if found, -1 otherwise. </returns>
        public int GetNextOccupiedSeatIndex(int start)
        {
            int index = start;

            for (int i = 0; i < MaxPlayers - 1; i++)
            {
                index++;
                index %= MaxPlayers;

                if (IsSeatOccupied(index)) return index;
            }

            return -1;
        }

        #endregion

        #region Player

        /// <summary> Adds the given player to the first empty seat, if there is any. </summary>
        /// <param name="player"> Player to be added to the table. </param>
        public void AddPlayer(TablePlayer player)
        {
            int index = GetFirstFreeSeatIndex();
            if (index < 0) return;

            _players[index] = player ?? throw new ArgumentNullException(nameof(player));
            _playerCount++;

            OnPlayerJoined(new PlayerJoinedEventArgs(player));
        }

        /// <summary> Finds the player with the given username and returns the index of that player's position on the table. </summary>
        /// <param name="username"> Username to be processed. </param>
        /// <returns> Index of the player with the given username. </returns>
        public int GetPlayerIndex(string username)
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (IsSeatOccupied(i) && _players[i].Username == username) return i;
            }

            return -1;
        }

        public TablePlayer GetPlayerAt(int index)
        {
            return _players[index];
        }

        /// <summary> Removes the specified player from the table. </summary>
        /// <param name="player"> Player to be removed. </param>
        /// <returns> True if the player was removed, false otherwise </returns>
        public void RemovePlayer(TablePlayer player)
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (IsSeatOccupied(i) && _players[i].Equals(player))
                {
                    _players[i] = null;
                    _playerCount--;

                    OnPlayerLeft(new PlayerLeftEventArgs(i));
                }
            }
        }

        /// <summary> Returns the copy of the internal table array. </summary>
        public TablePlayer[] GetPlayerArray()
        {
            TablePlayer[] players = new TablePlayer[_players.Length];
            Array.Copy(_players, players, _players.Length);
            return players;
        }

        #endregion

        /// <summary> Finds and returns the index of the first free seat, if there is one. </summary>
        /// <returns> Index of the first free seat if found, -1 if there are no free seats. </returns>
        public int GetFirstFreeSeatIndex()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (!IsSeatOccupied(i)) return i;
            }

            return -1;
        }

        public bool IsSeatOccupied(int index)
        {
            return _players[index] != null;
        }

        protected virtual void OnPlayerJoined(PlayerJoinedEventArgs args) => PlayerJoined?.Invoke(this, args);
        protected virtual void OnPlayerLeft(PlayerLeftEventArgs args) => PlayerLeft?.Invoke(this, args);
    }
}