namespace Poker.Cards
{
    /// <summary> An enumeration of all the possible hand values. </summary>
    public enum HandValue
    {
        /// <summary> #10: No combination. Lowest value hand. </summary>
        HighCard = 0,

        /// <summary> #9: Two cards of the same rank. </summary>
        OnePair = 1,

        /// <summary> #8: Pair + pair. </summary>
        TwoPair = 2,

        /// <summary> #7: Three of any rank. </summary>
        Threes = 3,

        /// <summary> #6: Five rank values in a row. </summary>
        Straight = 4,

        /// <summary> #5: Five cards of the same suit. </summary>
        Flush = 5,

        /// <summary> #4: Threes + pair. </summary>
        FullHouse = 6,

        /// <summary> #3: Four of any rank. </summary>
        Fours = 7,

        /// <summary> #2: Straight + flush (but not up to Ace). </summary>
        StraightFlush = 8,

        /// <summary> #1: A straight-flush up to Ace. Highest value hand. </summary>
        RoyalFlush = 9
    }
}