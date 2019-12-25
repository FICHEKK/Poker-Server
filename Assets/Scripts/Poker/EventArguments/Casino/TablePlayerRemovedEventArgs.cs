using System;

namespace Poker.EventArguments.Casino {
    public class TablePlayerRemovedEventArgs : EventArgs {
        public Table Table { get; }
        public string Username { get; }

        public TablePlayerRemovedEventArgs(Table table, string username) {
            Table = table;
            Username = username;
        }
    }
}