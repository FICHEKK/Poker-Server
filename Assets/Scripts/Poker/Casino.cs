using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Poker.Players;

namespace Poker {
    
    /// <summary>
    /// Models a casino, that is, the collection of multiple poker tables.
    /// </summary>
    public static class Casino {

        /// <summary>
        /// The number of active tables in this casino.
        /// </summary>
        public static int TableCount => TableByTitle.Count;
        
        /// <summary>
        /// A collection of all the active tables' names.
        /// </summary>
        public static IEnumerable<string> TableNames => TableByTitle.Keys;
        
        /// <summary>
        /// A collection of all the active tables.
        /// </summary>
        public static IEnumerable<Table> Tables => TableByTitle.Values;

        /// <summary>
        /// A thread-safe dictionary that maps table's name to its corresponding table.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Table> TableByTitle = new ConcurrentDictionary<string, Table>();
        
        /// <summary>
        /// Lock used by the lobby-players hash-set.
        /// </summary>
        private static readonly object LobbyPlayersPadlock = new object();
        
        /// <summary>
        /// Lock used by the table-players hash-set.
        /// </summary>
        private static readonly object TablePlayersPadlock = new object();
            
        /// <summary>
        /// A set of all the players currently in the lobby.
        /// </summary>
        private static readonly HashSet<LobbyPlayer> LobbyPlayers = new HashSet<LobbyPlayer>();
        
        /// <summary>
        /// A set of all the players currently on the table.
        /// </summary>
        private static readonly HashSet<TablePlayer> TablePlayers = new HashSet<TablePlayer>();

        /// <summary>
        /// Total number of players currently in the casino (in-lobby + on-table).
        /// </summary>
        public static int PlayerCount => LobbyPlayerCount + TablePlayerCount;
        
        /// <summary>
        /// Total number of players currently in the lobby.
        /// </summary>
        public static int LobbyPlayerCount { get { lock (LobbyPlayersPadlock) return LobbyPlayers.Count; } }
        
        /// <summary>
        /// Total number of players currently playing on all of the tables.
        /// </summary>
        public static int TablePlayerCount { get { lock (TablePlayersPadlock) return TablePlayers.Count; } }

        #region Table

        /// <summary>
        /// Adds a new table to this casino.
        /// </summary>
        /// <param name="title">New table's name.</param>
        /// <param name="table">Table to be added.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        public static bool AddTable(string title, Table table) {
            return TableByTitle.TryAdd(title, table);
        }
        
        /// <param name="title">Table's name.</param>
        /// <returns>The table with the specified name.</returns>
        public static Table GetTable(string title) {
            return TableByTitle[title];
        }
        
        /// <summary>
        /// Checks if the table with the given title exists.
        /// </summary>
        /// <param name="title">The title to be checked.</param>
        /// <returns>True if table with the given title exists, false otherwise.</returns>
        public static bool HasTableWithTitle(string title) {
            return TableByTitle.ContainsKey(title);
        }
        
        /// <summary>
        /// Removes the specified table from this casino.
        /// </summary>
        /// <param name="tableTitle">Table's name.</param>
        /// <returns>True if removed successfully, false otherwise.</returns>
        public static bool RemoveTable(string tableTitle) {
            return TableByTitle.TryRemove(tableTitle, out _);
        }

        #endregion
        
        #region Player in-lobby

        /// <summary>
        /// Adds a new player to the lobby.
        /// </summary>
        /// <param name="player">Player to be added.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        public static bool AddLobbyPlayer(LobbyPlayer player) {
            lock (LobbyPlayersPadlock) {
                return LobbyPlayers.Add(player);
            }
        }
        
        /// <summary>
        /// Returns the player with the specified username from the lobby.
        /// </summary>
        /// <param name="username">Player's username.</param>
        /// <returns>Player with the specified username if found, null otherwise.</returns>
        public static LobbyPlayer GetLobbyPlayer(string username) {
            lock (LobbyPlayersPadlock) {
                return LobbyPlayers.FirstOrDefault(player => player.Username == username);
            }
        }
        
        /// <summary>
        /// Removes the specified player from the lobby.
        /// </summary>
        /// <param name="player">Player to be removed.</param>
        /// <returns>True if removed successfully, false otherwise.</returns>
        public static bool RemoveLobbyPlayer(LobbyPlayer player) {
            lock (LobbyPlayersPadlock) {
                return LobbyPlayers.Remove(player);
            }
        }

        #endregion

        #region Player on-table

        /// <summary>
        /// Adds a new player to the collection of players that are currently on table.
        /// </summary>
        /// <param name="player">Player to be added.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        public static bool AddTablePlayer(TablePlayer player) {
            lock (TablePlayersPadlock) {
                return TablePlayers.Add(player);
            }
        }
        
        /// <summary>
        /// Returns the reference to the player with the specified username from that is currently on table.
        /// </summary>
        /// <param name="username">Player's username.</param>
        /// <returns>Player with the specified username if found, null otherwise.</returns>
        public static TablePlayer GetTablePlayer(string username) {
            lock (TablePlayersPadlock) {
                return TablePlayers.FirstOrDefault(player => player.Username == username);
            }
        }
        
        /// <summary>
        /// Removes the specified player from the collection of players that are currently on table.
        /// </summary>
        /// <param name="player">Player to be removed.</param>
        /// <returns>True if removed successfully, false otherwise.</returns>
        public static bool RemoveTablePlayer(TablePlayer player) {
            lock (TablePlayersPadlock) {
                return TablePlayers.Remove(player);
            }
        }

        #endregion

        #region Player methods
        
        public static bool HasPlayerWithUsername(string username) {
            return GetLobbyPlayer(username) != null || GetTablePlayer(username) != null;
        }

        public static void MovePlayerFromLobbyToTable(string username, Table table, int buyIn) {
            LobbyPlayer lobbyPlayer = GetLobbyPlayer(username);
            RemoveLobbyPlayer(lobbyPlayer);

            Seat seat = table.GetSeatAt(table.GetFirstFreeSeatIndex());
            TablePlayer tablePlayer = new TablePlayer(username, lobbyPlayer.ChipCount, seat, buyIn, lobbyPlayer.Reader, lobbyPlayer.Writer);
            AddTablePlayer(tablePlayer);

            table.AddPlayer(tablePlayer, buyIn);
        }

        public static void MovePlayerFromTableToLobby(string username) {
            TablePlayer tablePlayer = GetTablePlayer(username);
            RemoveTablePlayer(tablePlayer);
            
            LobbyPlayer lobbyPlayer = new LobbyPlayer(username, tablePlayer.ChipCount, tablePlayer.Reader, tablePlayer.Writer);
            AddLobbyPlayer(lobbyPlayer);
            
            tablePlayer.Seat.Table.RemovePlayer(tablePlayer);
        }

        #endregion
    }
}