using System;

namespace Poker.EventArguments.Casino {
    public class LobbyPlayerRemovedEventArgs : EventArgs {
        public string Username { get; }

        public LobbyPlayerRemovedEventArgs(string username) {
            Username = username;
        }
    }
}