using System;

namespace Poker.EventArguments {
    public class PlayerAddedEventArgs : EventArgs {
        public int Index { get; }
        public string Username { get; }
        public int ChipCount { get; }

        public PlayerAddedEventArgs(int index, string username, int chipCount) {
            Index = index;
            Username = username;
            ChipCount = chipCount;
        }
    }
}