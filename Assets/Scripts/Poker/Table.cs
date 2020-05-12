using System;
using System.Collections.Generic;
using Poker.Players;

namespace Poker
{
    /// <summary>Encapsulates data of a single poker table.</summary>
    public sealed class Table
    {
        /// <summary>Current dealer button index.</summary>
        public int DealerButtonIndex { get; private set; }

        /// <summary>Current number of players at the table.</summary>
        public int PlayerCount { get; private set; }

        /// <summary>Maximum number of players this table can seat.</summary>
        public int MaxPlayers { get; }
        
        /// <summary>Internal collection of players that are currently on this table.</summary>
        private readonly TablePlayer[] _players;
        
        /// <summary> Used for placing new players on the random position. </summary>
        private readonly Random _random = new Random();

        /// <summary> Constructs a new table with the given capacity. </summary>
        /// <param name="maxPlayers"> Maximum number of players (capacity). </param>
        public Table(int maxPlayers)
        {
            MaxPlayers = maxPlayers;
            _players = new TablePlayer[MaxPlayers];
        }

        /// <summary> Increments the button position, skipping all the empty seats on the way. </summary>
        public void IncrementButtonIndex()
        {
            DealerButtonIndex = GetNextOccupiedSeatIndex(DealerButtonIndex);

            if (DealerButtonIndex == -1)
            {
                DealerButtonIndex = 0;
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

        /// <summary> Adds the given player to the first empty seat, if there is any. </summary>
        /// <param name="player"> Player to be added to the table. </param>
        public void AddPlayer(TablePlayer player)
        {
            var index = GetRandomFreeSeatIndex();
            if (index < 0) return;

            player.Index = index;
            _players[index] = player;
            PlayerCount++;
        }

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
                    PlayerCount--;
                    break;
                }
            }
        }
        
        /// <summary>Returns the player at the specified index.</summary>
        /// <param name="index">Index of the player.</param>
        public TablePlayer this[int index] => _players[index];

        /// <summary> Returns the copy of the internal table array. </summary>
        public TablePlayer[] GetPlayerArray()
        {
            var players = new TablePlayer[_players.Length];
            Array.Copy(_players, players, _players.Length);
            return players;
        }

        /// <summary> Finds and returns the index of the first free seat, if there is one. </summary>
        /// <returns> Index of the first free seat if found, -1 if there are no free seats. </returns>
        private int GetFirstFreeSeatIndex()
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (_players[i] == null) return i;
            }

            return -1;
        }

        /// <summary> Finds and returns the index of the random free seat, if there is one. </summary>
        /// <returns> Index of the random free seat if there are any, -1 if there are no free seats. </returns>
        private int GetRandomFreeSeatIndex()
        {
            var freeSeatIndexes = new List<int>();
            
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (_players[i] == null) freeSeatIndexes.Add(i);
            }

            return freeSeatIndexes.Count > 0 ? freeSeatIndexes[_random.Next(freeSeatIndexes.Count)] : -1;
        }
        
        /// <summary> Returns an array of all the currently active clients at the this table. </summary>
        public Client[] GetActiveClients()
        {
            var clients = new Client[PlayerCount];
            
            for (int i = 0, insertIndex = 0; i < MaxPlayers; i++)
            {
                if(_players[i] == null) continue;
                clients[insertIndex++] = _players[i].Client;
            }

            return clients;
        }

        /// <summary>Enumerator used to iterate this table's current players.</summary>
        public IEnumerator<TablePlayer> GetEnumerator()
        {
            for (var i = 0; i < MaxPlayers; i++)
            {
                if(_players[i] == null) continue;
                yield return _players[i];
            }
        }
    }
}