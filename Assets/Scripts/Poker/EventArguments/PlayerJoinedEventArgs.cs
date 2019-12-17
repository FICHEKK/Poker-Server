using System;

namespace Poker.EventArguments {
    public class PlayerJoinedEventArgs : EventArgs {
        public int Index { get; }
        public string Username { get; }
        public int Stack { get; }

        public PlayerJoinedEventArgs(int index, string username, int stack) {
            Index = index;
            Username = username;
            Stack = stack;
        }
    }
}