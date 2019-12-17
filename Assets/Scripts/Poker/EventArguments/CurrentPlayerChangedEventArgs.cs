using System;

namespace Poker.EventArguments {
    public class CurrentPlayerChangedEventArgs : EventArgs {
        public int CurrentPlayerIndex { get; }

        public CurrentPlayerChangedEventArgs(int currentPlayerIndex) {
            CurrentPlayerIndex = currentPlayerIndex;
        }
    }
}