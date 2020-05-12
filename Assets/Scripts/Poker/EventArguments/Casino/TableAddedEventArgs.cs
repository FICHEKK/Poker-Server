using System;
using Poker.TableControllers;

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