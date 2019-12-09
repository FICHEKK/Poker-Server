namespace Poker.Cards {
    /// <summary>
    /// An enumeration of all the possible hand values.
    /// </summary>
    public enum HandValue {
    
        /// <summary>
        /// <para>#10: No combination. Lowest value hand.</para>
        /// High-card example: 3H 6S 7D 10S KC
        /// </summary>
        HighCard = 0,
    
        /// <summary>
        /// <para>#9: Two cards of the same rank.</para>
        /// One-pair example: 2D 2S 5H 8S 9H
        /// </summary>
        OnePair = 1,
    
        /// <summary>
        /// <para>#8: Pair + pair.</para>
        /// Two-pair example: 6H 6D 7S 7D JS
        /// </summary>
        TwoPair = 2,
    
        /// <summary>
        /// <para>#7: Three of any rank.</para>
        /// Threes example: 9D 9H 9S KH AD
        /// </summary>
        Threes = 3,
    
        /// <summary>
        /// <para>#6: Five rank values in a row.</para>
        /// Straight example: 2S 3H 4D 5S 6H
        /// </summary>
        Straight = 4,
    
        /// <summary>
        /// <para>#5: Five cards of the same suit.</para>
        /// Flush example: 2D 4D 6D 9D KD
        /// </summary>
        Flush = 5,
    
        /// <summary>
        /// <para>#4: Threes + pair.</para>
        /// Full-house example: 4D 4H 4S QD QS
        /// </summary>
        FullHouse = 6,
    
        /// <summary>
        /// <para>#3: Four of any rank.</para>
        /// Fours example: 7H 7C 7D 7S 2H
        /// </summary>
        Fours = 7,
    
        /// <summary>
        /// <para>#2: Straight + flush (but not up to Ace).</para>
        /// Straight-flush example: 3H 4H 5H 6H 7H
        /// </summary>
        StraightFlush = 8,
    
        /// <summary>
        /// <para>#1: A straight-flush up to Ace. Highest value hand.</para>
        /// Royal-flush example: 10H JH QH KH AH
        /// </summary>
        RoyalFlush = 9
    }
}