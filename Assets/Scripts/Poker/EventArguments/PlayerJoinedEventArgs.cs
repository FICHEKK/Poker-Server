using System;

namespace Poker.EventArguments {
    public class PlayerJoinedEventArgs : EventArgs {
        public int Index { get; }
        public string Username { get; }
        public int ChipCount { get; }

        public PlayerJoinedEventArgs(int index, string username, int chipCount) {
            Index = index;
            Username = username;
            ChipCount = chipCount;
        }
    }
}