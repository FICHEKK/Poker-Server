using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Poker {
    
    /// <summary>
    /// Models a casino, that is, the collection of multiple poker tables.
    /// </summary>
    public static class Casino {

        /// <summary>
        /// The number of active tables in this casino.
        /// </summary>
        public static int TableCount => _tables.Count;
        
        /// <summary>
        /// A collection of all the active tables' names.
        /// </summary>
        public static IEnumerable<string> TableNames => _tables.Keys;
        
        /// <summary>
        /// A collection of all the active tables.
        /// </summary>
        public static IEnumerable<Table> Tables => _tables.Values;

        /// <summary>
        /// A thread-safe dictionary that maps table's name to its corresponding table.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Table> _tables = new ConcurrentDictionary<string, Table>();
        
        /// <summary>
        /// Lock used by the player hash-set.
        /// </summary>
        private static readonly object SetLock = new object();
            
        /// <summary>
        /// A set of all the players in the casino.
        /// </summary>
        private static readonly HashSet<string> Players = new HashSet<string>();

        /// <summary>
        /// Current number of players in the casino.
        /// </summary>
        public static int PlayerCount {
            get {
                lock (SetLock) {
                    return Players.Count;
                }
            }
        }

        /// <param name="tableTitle">Table's name.</param>
        /// <returns>The table with the specified name.</returns>
        public static Table GetTable(string tableTitle) {
            return _tables[tableTitle];
        }

        /// <summary>
        /// Adds a new table to this casino.
        /// </summary>
        /// <param name="tableTitle">New table's name.</param>
        /// <param name="table">New table.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        public static bool AddTable(string tableTitle, Table table) {
            return _tables.TryAdd(tableTitle, table);
        }

        /// <summary>
        /// Removes the specified table from this casino.
        /// </summary>
        /// <param name="tableTitle">Table's name.</param>
        /// <returns>True if removed successfully, false otherwise.</returns>
        public static bool RemoveTable(string tableTitle) {
            return _tables.TryRemove(tableTitle, out _);
        }

        /// <summary>
        /// Removes all the tables from this casino.
        /// </summary>
        public static void RemoveAllTables() {
            _tables.Clear();
        }

        /// <summary>
        /// Checks if the table with the given title exists.
        /// </summary>
        /// <param name="title">The title to be checked.</param>
        /// <returns>True if table with the given title exists, false otherwise.</returns>
        public static bool HasTableWithTitle(string title) {
            return _tables.ContainsKey(title);
        }

        /// <summary>
        /// Adds a new player to the casino.
        /// </summary>
        /// <param name="username">Player's username.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        public static bool AddPlayer(string username) {
            lock (SetLock) {
                return Players.Add(username);
            }
        }

        /// <summary>
        /// Removes player with the given username from the casino.
        /// </summary>
        /// <param name="username">Player's username.</param>
        /// <returns>True if removed successfully, false otherwise.</returns>
        public static bool RemovePlayer(string username) {
            lock (SetLock) {
                return Players.Remove(username);
            }
        }

        /// <summary>
        /// Checks if the player with the given username exists.
        /// </summary>
        /// <param name="username">Username to be checked.</param>
        /// <returns></returns>
        public static bool HasPlayerWithUsername(string username) {
            lock (SetLock) {
                return Players.Contains(username);
            }
        }
    }
}