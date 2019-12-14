using System;

namespace Poker.EventArguments {
    public class PlayerRemovedEventArgs : EventArgs {
        public int Index { get; }

        public PlayerRemovedEventArgs(int index) {
            Index = index;
        }
    }
}