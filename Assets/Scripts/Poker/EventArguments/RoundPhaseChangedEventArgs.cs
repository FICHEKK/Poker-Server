using System;

namespace Poker.EventArguments
{
    public class RoundPhaseChangedEventArgs : EventArgs
    {
        public Round.Phase CurrentPhase { get; }

        public RoundPhaseChangedEventArgs(Round.Phase currentPhase)
        {
            CurrentPhase = currentPhase;
        }
    }
}