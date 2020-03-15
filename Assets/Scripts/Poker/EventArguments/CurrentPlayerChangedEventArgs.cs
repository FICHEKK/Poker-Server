using System;

namespace Poker.EventArguments
{
    public class CurrentPlayerChangedEventArgs : EventArgs
    {
        public int CurrentPlayerIndex { get; }
        public int RequiredCall { get; }
        public int MinRaise { get; }
        public int MaxRaise { get; }

        public CurrentPlayerChangedEventArgs(int currentPlayerIndex, int requiredCall, int minRaise, int maxRaise)
        {
            CurrentPlayerIndex = currentPlayerIndex;
            RequiredCall = requiredCall;
            MinRaise = minRaise;
            MaxRaise = maxRaise;
        }
    }
}