using System;

namespace Poker.EventArguments.Casino
{
    public class LobbyPlayerAddedEventArgs : EventArgs
    {
        public string Username { get; }
        public int ChipCount { get; }

        public LobbyPlayerAddedEventArgs(string username, int chipCount)
        {
            Username = username;
            ChipCount = chipCount;
        }
    }
}