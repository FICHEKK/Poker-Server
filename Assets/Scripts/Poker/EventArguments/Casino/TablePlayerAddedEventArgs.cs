using System;
using Poker.TableControllers;

namespace Poker.EventArguments.Casino
{
    public class TablePlayerAddedEventArgs : EventArgs
    {
        public TableController TableController { get; }
        public string Username { get; }

        public TablePlayerAddedEventArgs(TableController tableController, string username)
        {
            TableController = tableController;
            Username = username;
        }
    }
}