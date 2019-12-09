using System;
using System.Text;

namespace Poker.Cards {
    /// <summary>
    /// A simple card model that encapsulates card's rank and suit.
    /// </summary>
    public class Card : IComparable<Card> {
    
        /// <summary>
        /// This card's rank.
        /// </summary>
        public Rank Rank { get; }
    
        /// <summary>
        /// This card's suit.
        /// </summary>
        public Suit Suit { get; }

        /// <summary>
        /// Constructs a new card with the given rank and suit.
        /// </summary>
        public Card(Rank rank, Suit suit) {
            Rank = rank;
            Suit = suit;
        }

        /// <summary>
        /// Compares this card with the given card. Comparison is entirely
        /// rank based.
        /// Ranks from strongest to weakest: A, K, Q, J, 10, 9, 8, 7, 6, 5, 4, 3, 2.
        /// </summary>
        /// 
        /// <returns>
        /// 1 if this card has higher rank or the provided card is null,
        /// 0 if both ranks are equal,
        /// -1 if this card has lower rank
        /// </returns>
        public int CompareTo(Card other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            if ((int) Rank > (int) other.Rank) return 1;
            if (Rank == other.Rank) return 0;
        
            return -1;
        }
    
        /// <summary>
        /// Returns the string representation of this card.
        /// </summary>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            switch (Rank) {
                case Rank.Two:   sb.Append("2"); break;
                case Rank.Three: sb.Append("3"); break;
                case Rank.Four:  sb.Append("4"); break;
                case Rank.Five:  sb.Append("5"); break;
                case Rank.Six:   sb.Append("6"); break;
                case Rank.Seven: sb.Append("7"); break;
                case Rank.Eight: sb.Append("8"); break;
                case Rank.Nine:  sb.Append("9"); break;
                case Rank.Ten:   sb.Append("10"); break;
                case Rank.Jack:  sb.Append("J"); break;
                case Rank.Queen: sb.Append("Q"); break;
                case Rank.King:  sb.Append("K"); break;
                case Rank.Ace:   sb.Append("A"); break;
            }

            switch (Suit) {
                case Suit.Heart:   sb.Append("H"); break;
                case Suit.Diamond: sb.Append("D"); break;
                case Suit.Spade:   sb.Append("S"); break;
                case Suit.Club:    sb.Append("C"); break;
            }

            return sb.ToString();
        }
    }
}
