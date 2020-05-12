using System;

namespace Poker.EventArguments.Casino
{
    public class TableAddedEventArgs : EventArgs
    {
        public TableController TableController { get; }

        public TableAddedEventArgs(TableController tableController)
        {
            TableController = tableController;
        }
    }
}