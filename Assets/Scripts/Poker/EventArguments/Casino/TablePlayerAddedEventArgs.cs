using System;

namespace Poker.EventArguments.Casino {
    public class TablePlayerAddedEventArgs : EventArgs {
        public Table Table { get; }
        public string Username { get; }

        public TablePlayerAddedEventArgs(Table table, string username) {
            Table = table;
            Username = username;
        }
    }
}