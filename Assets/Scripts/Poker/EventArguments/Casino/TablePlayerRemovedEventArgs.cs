using System;
using Poker.TableControllers;

namespace Poker.EventArguments.Casino
{
    public class TablePlayerRemovedEventArgs : EventArgs
    {
        public TableController TableController { get; }
        public string Username { get; }

        public TablePlayerRemovedEventArgs(TableController tableController, string username)
        {
            TableController = tableController;
            Username = username;
        }
    }
}