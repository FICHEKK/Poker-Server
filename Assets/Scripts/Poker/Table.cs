using System;
using System.Collections.Concurrent;
using System.Threading;
using Poker.EventArguments;
using Poker.Players;
using RequestProcessors;

namespace Poker
{
    /// <summary>
    /// Encapsulates data of a single poker table.
    /// </summary>
    public sealed class Table
    {
        /// <summary>A blocking queue used to store unprocessed client requests.</summary>
        public BlockingCollection<IClientRequestProcessor> RequestProcessors { get; } = new BlockingCollection<IClientRequestProcessor>();

        /// <summary>A dummy action that is used to indicate the end of the consuming.</summary>
        private readonly IClientRequestProcessor _poisonPill = new PoisonPill();
        
        /// <summary>Event that gets raised every time a player joins this table.</summary>
        public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;

        /// <summary>Event that gets raised every time a player leaves this table.</summary>
        public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

        /// <summary>This table's title (name).</summary>
        public string Title { get; }

        /// <summary>This table's small blind.</summary>
        public int SmallBlind { get; }
        
        /// <summary>Indicates whether this table is ranked.</summary>
        public bool IsRanked { get; }

        /// <summary>Current dealer button index.</summary>
        public int DealerButtonIndex => _dealerButtonIndex;

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
        private int _dealerButtonIndex;
        private int _playerCount;

        /// <summary> Constructs a new table with the given small blind and maximum number of players. </summary>
        /// <param name="title"> This table's title (name). </param>
        /// <param name="smallBlind"> The small blind. </param>
        /// <param name="maxPlayers"> Maximum number of players. </param>
        /// <param name="isRanked"> Indicates whether this table is ranked. </param>
        public Table(string title, int smallBlind, int maxPlayers, bool isRanked = false)
        {
            Title = title;
            SmallBlind = smallBlind;
            MaxPlayers = maxPlayers;
            IsRanked = isRanked;
            Dealer = new Dealer(this);
            _players = new TablePlayer[MaxPlayers];
            
            new Thread(ConsumeClientRequests).Start();
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

        #region Button

        /// <summary> Increments the button position, skipping all the empty seats on the way. </summary>
        public void IncrementButtonIndex()
        {
            _dealerButtonIndex = GetNextOccupiedSeatIndex(_dealerButtonIndex);

            if (_dealerButtonIndex == -1)
            {
                _dealerButtonIndex = 0;
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

                if (_players[index] != null) return index;
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
            
            PlayerJoined?.Invoke(this, new PlayerJoinedEventArgs(player));
        }
        
        /// <summary>Returns the player at the specified index.</summary>
        /// <param name="index">Index of the player.</param>
        public TablePlayer this[int index] => _players[index];

        /// <summary> Removes the specified player from the table. </summary>
        /// <param name="player"> Player to be removed. </param>
        /// <returns> True if the player was removed, false otherwise </returns>
        public void RemovePlayer(TablePlayer player)
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (_players[i] != null && _players[i].Equals(player))
                {
                    _players[i] = null;
                    _playerCount--;

                    PlayerLeft?.Invoke(this, new PlayerLeftEventArgs(i));
                    break;
                }
            }
        }

        /// <summary> Returns the copy of the internal table array. </summary>
        public TablePlayer[] GetPlayerArray()
        {
            var players = new TablePlayer[_players.Length];
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
                if (_players[i] == null) return i;
            }

            return -1;
        }
        
        /// <summary> Returns an array of all the currently active clients at the this table. </summary>
        public Client[] GetActiveClients()
        {
            var clients = new Client[_playerCount];
            
            for (int i = 0, insertIndex = 0; i < MaxPlayers; i++)
            {
                if(_players[i] == null) continue;
                clients[insertIndex++] = _players[i].Client;
            }

            return clients;
        }
    }
}