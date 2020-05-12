using System;
using System.Collections.Concurrent;
using Poker.EventArguments.Casino;
using Poker.Players;
using Poker.TableControllers;

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

        /// <summary>A thread-safe dictionary that maps table's title to its corresponding table controller.</summary>
        public static readonly ConcurrentDictionary<string, TableController> TableControllers =
            new ConcurrentDictionary<string, TableController>();

        /// <summary>A collection of all the players currently in the lobby.</summary>
        private static readonly ConcurrentDictionary<string, LobbyPlayer> LobbyPlayers =
            new ConcurrentDictionary<string, LobbyPlayer>();

        /// <summary>A collection of all the players currently on the table.</summary>
        private static readonly ConcurrentDictionary<string, TablePlayer> TablePlayers =
            new ConcurrentDictionary<string, TablePlayer>();

        static Casino()
        {
            AddTableController(new StandardTableController(new Table(2), "Poor Player Penthouse", 1));
            AddTableController(new StandardTableController(new Table(2), "Casual Playa Secret Room", 5));
            AddTableController(new StandardTableController(new Table(2), "On The Rise", 20));
            AddTableController(new StandardTableController(new Table(2), "A Middle Class Table", 50));
            AddTableController(new StandardTableController(new Table(2), "Local Tourney", 100));
            AddTableController(new RankedTableController(new Table(2), "A Radical Table", 500));
            AddTableController(new RankedTableController(new Table(2), "Las Vegas Baller", 2000));
            AddTableController(new RankedTableController(new Table(2), "WSOP High Rollers", 10000));
        }

        //============================================================
        //                     Player in-lobby
        //============================================================

        public static void AddLobbyPlayer(LobbyPlayer player)
        {
            LobbyPlayers[player.Username] = player;
            LobbyPlayerAdded?.Invoke(null, new LobbyPlayerAddedEventArgs(player.Username, player.ChipCount));
        }

        public static void RemoveLobbyPlayer(LobbyPlayer player)
        {
            LobbyPlayers.TryRemove(player.Username, out _);
            LobbyPlayerRemoved?.Invoke(null, new LobbyPlayerRemovedEventArgs(player.Username));
        }

        public static LobbyPlayer GetLobbyPlayer(string username) =>
            LobbyPlayers.TryGetValue(username, out var player) ? player : null;

        //============================================================
        //                      Player on-table
        //============================================================

        public static void AddTablePlayer(TablePlayer player)
        {
            TablePlayers[player.Username] = player;
            TablePlayerAdded?.Invoke(null, new TablePlayerAddedEventArgs(player.TableController, player.Username));
        }
        
        public static void RemoveTablePlayer(TablePlayer player)
        {
            TablePlayers.TryRemove(player.Username, out _);
            TablePlayerRemoved?.Invoke(null, new TablePlayerRemovedEventArgs(player.TableController, player.Username));
        }
        
        public static TablePlayer GetTablePlayer(string username) =>
            TablePlayers.TryGetValue(username, out var player) ? player : null;

        //============================================================
        //                       Table methods
        //============================================================

        public static void AddTableController(TableController controller)
        {
            TableControllers[controller.Title] = controller;
            TableAdded?.Invoke(null, new TableAddedEventArgs(controller));
        }
        
        public static void RemoveTableController(string title)
        {
            TableControllers.TryRemove(title, out _);
            TableRemoved?.Invoke(null, new TableRemovedEventArgs(title));
        }

        public static TableController GetTableController(string title) =>
            TableControllers.TryGetValue(title, out var controller) ? controller : null;

        //============================================================
        //                      Helper methods
        //============================================================

        public static bool HasPlayerWithUsername(string username) => LobbyPlayers.ContainsKey(username) || TablePlayers.ContainsKey(username);
        public static bool HasTableWithTitle(string title) => TableControllers.ContainsKey(title);
    }
}