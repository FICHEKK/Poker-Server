using System;

namespace Poker.EventArguments
{
    public class CurrentPlayerChangedEventArgs : EventArgs
    {
        public int CurrentPlayerIndex { get; }
        public int RequiredCallAmount { get; }

        public CurrentPlayerChangedEventArgs(int currentPlayerIndex, int requiredCallAmount)
        {
            CurrentPlayerIndex = currentPlayerIndex;
            RequiredCallAmount = requiredCallAmount;
        }
    }
}