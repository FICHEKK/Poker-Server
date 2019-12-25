using System;

namespace Poker.EventArguments.Casino {
    public class TableAddedEventArgs : EventArgs {
        public Table Table { get; }

        public TableAddedEventArgs(Table table) {
            Table = table;
        }
    }
}