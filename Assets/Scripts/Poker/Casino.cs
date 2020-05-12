using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Poker.EventArguments.Casino;
using Poker.Players;

namespace Poker
{
    /// <summary>Models a casino, that is, the collection of multiple poker tables.</summary>
    public static class Casino
    {
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
        public static int TableCount => TableControllerByTitle.Count;

        /// <summary>A collection of all the active tables' names.</summary>
        public static IEnumerable<string> TableNames => TableControllerByTitle.Keys;

        /// <summary>A collection of all the active tables.</summary>
        public static IEnumerable<TableController> TableControllers => TableControllerByTitle.Values;

        /// <summary>A thread-safe dictionary that maps table's name to its corresponding table controller.</summary>
        private static readonly ConcurrentDictionary<string, TableController> TableControllerByTitle = new ConcurrentDictionary<string, TableController>();

        /// <summary>Lock used by the lobby-players hash-set.</summary>
        private static readonly object LobbyPlayersPadlock = new object();

        /// <summary>Lock used by the table-players hash-set.</summary>
        private static readonly object TablePlayersPadlock = new object();

        /// <summary>A set of all the players currently in the lobby.</summary>
        private static readonly HashSet<LobbyPlayer> LobbyPlayers = new HashSet<LobbyPlayer>();

        /// <summary>A set of all the players currently on the table.</summary>
        private static readonly HashSet<TablePlayer> TablePlayers = new HashSet<TablePlayer>();

        static Casino()
        {
            AddTableController(new StandardTableController(new Table(10), "Poor Player Penthouse", 1));
            AddTableController(new StandardTableController(new Table(10), "Casual Playa Secret Room", 5));
            AddTableController(new StandardTableController(new Table(10), "On The Rise", 20));
            AddTableController(new StandardTableController(new Table(10), "A Middle Class Table", 50));
            AddTableController(new StandardTableController(new Table(10), "Local Tourney", 100));
            AddTableController(new StandardTableController(new Table(10), "A Radical Table", 500));
            AddTableController(new StandardTableController(new Table(10), "Las Vegas Baller", 2000));
            AddTableController(new StandardTableController(new Table(10), "WSOP High Rollers", 10000));
        }

        //----------------------------------------------------------------
        //                      Player in-lobby
        //----------------------------------------------------------------

        public static void AddLobbyPlayer(LobbyPlayer player)
        {
            lock (LobbyPlayersPadlock)
            {
                LobbyPlayers.Add(player);
            }

            LobbyPlayerAdded?.Invoke(null, new LobbyPlayerAddedEventArgs(player.Username, player.ChipCount));
        }

        public static LobbyPlayer GetLobbyPlayer(string username)
        {
            lock (LobbyPlayersPadlock)
            {
                return LobbyPlayers.FirstOrDefault(player => player.Username == username);
            }
        }

        public static void RemoveLobbyPlayer(LobbyPlayer player)
        {
            lock (LobbyPlayersPadlock)
            {
                LobbyPlayers.Remove(player);
            }

            LobbyPlayerRemoved?.Invoke(null, new LobbyPlayerRemovedEventArgs(player.Username));
        }

        //----------------------------------------------------------------
        //                      Player on-table
        //----------------------------------------------------------------

        public static void AddTablePlayer(TablePlayer player)
        {
            lock (TablePlayersPadlock)
            {
                TablePlayers.Add(player);
            }

            TablePlayerAdded?.Invoke(null, new TablePlayerAddedEventArgs(player.TableController, player.Username));
        }

        public static TablePlayer GetTablePlayer(string username)
        {
            lock (TablePlayersPadlock)
            {
                return TablePlayers.FirstOrDefault(player => player.Username == username);
            }
        }

        public static void RemoveTablePlayer(TablePlayer player)
        {
            lock (TablePlayersPadlock)
            {
                TablePlayers.Remove(player);
            }

            TablePlayerRemoved?.Invoke(null, new TablePlayerRemovedEventArgs(player.TableController, player.Username));
        }

        //----------------------------------------------------------------
        //                      Table methods
        //----------------------------------------------------------------

        public static void AddTableController(TableController controller)
        {
            TableControllerByTitle.TryAdd(controller.Title, controller);
            TableAdded?.Invoke(null, new TableAddedEventArgs(controller));
        }

        public static TableController GetTableController(string title)
        {
            return TableControllerByTitle[title];
        }

        public static bool HasTableWithTitle(string title)
        {
            return TableControllerByTitle.ContainsKey(title);
        }

        public static void RemoveTableController(string title)
        {
            TableControllerByTitle.TryRemove(title, out _);
            TableRemoved?.Invoke(null, new TableRemovedEventArgs(title));
        }

        //----------------------------------------------------------------
        //                      Player methods
        //----------------------------------------------------------------

        public static bool HasPlayerWithUsername(string username)
        {
            return GetLobbyPlayer(username) != null || GetTablePlayer(username) != null;
        }
    }
}