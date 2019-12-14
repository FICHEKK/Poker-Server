namespace Poker {
    
    /// <summary>
    /// Models table round phases.
    /// </summary>
    public enum TablePhase {
        
        /// <summary>
        /// Waiting for at least 2 players to start the round.
        /// </summary>
        Waiting,
        
        /// <summary>
        /// First betting round where the hand cards are dealt.
        /// </summary>
        PreFlop,
        
        /// <summary>
        /// Second betting round after the flop cards were revealed.
        /// </summary>
        Flop,
        
        /// <summary>
        /// Third betting round after the turn card was revealed.
        /// </summary>
        Turn,
        
        /// <summary>
        /// Fourth betting round after the river card was revealed.
        /// </summary>
        River
    }
}