using System;

namespace Poker.EventArguments.Casino {
    public class TableRemovedEventArgs : EventArgs {
        public string Title { get; }

        public TableRemovedEventArgs(string title) {
            Title = title;
        }
    }
}