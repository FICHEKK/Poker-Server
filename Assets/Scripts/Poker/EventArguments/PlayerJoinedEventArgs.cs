using System;
using Poker.Players;

namespace Poker.EventArguments
{
    public class PlayerJoinedEventArgs : EventArgs
    {
        public TablePlayer Player { get; }

        public PlayerJoinedEventArgs(TablePlayer player)
        {
            Player = player;
        }
    }
}