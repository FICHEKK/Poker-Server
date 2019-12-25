using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Poker.EventArguments.Casino;
using Poker.Players;

namespace Poker {
    
    /// <summary>Models a casino, that is, the collection of multiple poker tables.</summary>
    public static class Casino {
        
        /// <summary> Raised each time a player joins the lobby. </summary>
        public static event EventHandler<LobbyPlayerAddedEventArgs> LobbyPlayerAdded;

        /// <summary> Raised each time a player leaves the lobby. </summary>
        public static event EventHandler<LobbyPlayerRemovedEventArgs> LobbyPlayerRemoved;

        /// <summary> Raised each time a player joins a table. </summary>
        public static event EventHandler<TablePlayerAddedEventArgs> TablePlayerAdded;

        /// <summary> Raised each time a player leaves a table. </summary>
        public static event EventHandler<TablePlayerRemovedEventArgs> TablePlayerRemoved;

        /// <summary> Raised each time a new table is created. </summary>
        public static event EventHandler<TableAddedEventArgs> TableAdded;

        /// <summary> Raised each time a table is removed. </summary>
        public static event EventHandler<TableRemovedEventArgs> TableRemoved;

        /// <summary>The number of active tables in this casino.</summary>
        public static int TableCount => TableByTitle.Count;

        /// <summary>Total number of players currently in the casino (in-lobby + on-table).</summary>
        public static int PlayerCount => LobbyPlayerCount + TablePlayerCount;

        /// <summary>Total number of players currently in the lobby.</summary>
        public static int LobbyPlayerCount { get { lock (LobbyPlayersPadlock) return LobbyPlayers.Count; } }

        /// <summary>Total number of players currently playing on all of the tables.</summary>
        public static int TablePlayerCount { get { lock (TablePlayersPadlock) return TablePlayers.Count; } }

        /// <summary>A collection of all the active tables' names.</summary>
        public static IEnumerable<string> TableNames => TableByTitle.Keys;

        /// <summary>A collection of all the active tables.</summary>
        public static IEnumerable<Table> Tables => TableByTitle.Values;

        /// <summary>A thread-safe dictionary that maps table's name to its corresponding table.</summary>
        private static readonly ConcurrentDictionary<string, Table> TableByTitle = new ConcurrentDictionary<string, Table>();

        /// <summary>Lock used by the lobby-players hash-set.</summary>
        private static readonly object LobbyPlayersPadlock = new object();

        /// <summary>Lock used by the table-players hash-set.</summary>
        private static readonly object TablePlayersPadlock = new object();

        /// <summary>A set of all the players currently in the lobby.</summary>
        private static readonly HashSet<LobbyPlayer> LobbyPlayers = new HashSet<LobbyPlayer>();

        /// <summary>A set of all the players currently on the table.</summary>
        private static readonly HashSet<TablePlayer> TablePlayers = new HashSet<TablePlayer>();
        
        #region Player in-lobby

        public static void AddLobbyPlayer(LobbyPlayer player) {
            lock (LobbyPlayersPadlock) {
                LobbyPlayers.Add(player);
            }
            
            LobbyPlayerAdded?.Invoke(null, new LobbyPlayerAddedEventArgs(player.Username, player.ChipCount));
        }

        public static LobbyPlayer GetLobbyPlayer(string username) {
            lock (LobbyPlayersPadlock) {
                return LobbyPlayers.FirstOrDefault(player => player.Username == username);
            }
        }

        public static void RemoveLobbyPlayer(LobbyPlayer player) {
            lock (LobbyPlayersPadlock) {
                LobbyPlayers.Remove(player);
            }
            
            LobbyPlayerRemoved?.Invoke(null, new LobbyPlayerRemovedEventArgs(player.Username));
        }

        #endregion

        #region Player on-table

        public static void AddTablePlayer(TablePlayer player) {
            lock (TablePlayersPadlock) {
                TablePlayers.Add(player);
            }
            
            TablePlayerAdded?.Invoke(null, new TablePlayerAddedEventArgs(player.Table, player.Username));
        }

        public static TablePlayer GetTablePlayer(string username) {
            lock (TablePlayersPadlock) {
                return TablePlayers.FirstOrDefault(player => player.Username == username);
            }
        }

        public static void RemoveTablePlayer(TablePlayer player) {
            lock (TablePlayersPadlock) {
                TablePlayers.Remove(player);
            }
            
            TablePlayerRemoved?.Invoke(null, new TablePlayerRemovedEventArgs(player.Table, player.Username));
        }

        #endregion
        
        #region Table

        public static void AddTable(string title, Table table) {
            TableByTitle.TryAdd(title, table);
            TableAdded?.Invoke(null, new TableAddedEventArgs(table));
        }

        public static Table GetTable(string title) {
            return TableByTitle[title];
        }

        public static bool HasTableWithTitle(string title) {
            return TableByTitle.ContainsKey(title);
        }

        public static void RemoveTable(string title) {
            TableByTitle.TryRemove(title, out _);
            TableRemoved?.Invoke(null, new TableRemovedEventArgs(title));
        }

        #endregion

        #region Player methods

        public static bool HasPlayerWithUsername(string username) {
            return GetLobbyPlayer(username) != null || GetTablePlayer(username) != null;
        }

        public static void MovePlayerFromLobbyToTable(string username, Table table, int buyIn) {
            LobbyPlayer lobbyPlayer = GetLobbyPlayer(username);
            RemoveLobbyPlayer(lobbyPlayer);

            int index = table.GetFirstFreeSeatIndex();
            TablePlayer tablePlayer = new TablePlayer(username, lobbyPlayer.ChipCount, table, buyIn, index, lobbyPlayer.Reader, lobbyPlayer.Writer);
            AddTablePlayer(tablePlayer);

            table.AddPlayer(tablePlayer, buyIn);
        }

        public static void MovePlayerFromTableToLobby(string username) {
            TablePlayer tablePlayer = GetTablePlayer(username);
            RemoveTablePlayer(tablePlayer);

            LobbyPlayer lobbyPlayer = new LobbyPlayer(username, tablePlayer.ChipCount, tablePlayer.Reader, tablePlayer.Writer);
            AddLobbyPlayer(lobbyPlayer);

            tablePlayer.Table.RemovePlayer(tablePlayer);
        }

        #endregion
    }
}