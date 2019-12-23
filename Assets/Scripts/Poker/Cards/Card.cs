using System;
using System.Text;

namespace Poker.Cards {
    
    /// <summary> A simple card model that encapsulates card's rank and suit. </summary>
    public class Card : IComparable<Card> {
        
        /// <summary> This card's rank. </summary>
        public Rank Rank { get; }

        /// <summary> This card's suit. </summary>
        public Suit Suit { get; }

        /// <summary> Constructs a new card with the given rank and suit. </summary>
        public Card(Rank rank, Suit suit) {
            Rank = rank;
            Suit = suit;
        }

        /// <summary> Compares this card with the given card. Comparison is entirely rank based. </summary>
        public int CompareTo(Card other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            if ((int) Rank > (int) other.Rank) return 1;
            if (Rank == other.Rank) return 0;

            return -1;
        }
        
        public static Card Parse(string s) {
            if (s == null) throw new FormatException("Cannot parse null string");

            var chars = s.ToCharArray();

            Rank rank;
            if (s.Length == 2) {
                switch (chars[0]) {
                    case '2': rank = Rank.Two; break;
                    case '3': rank = Rank.Three; break;
                    case '4': rank = Rank.Four; break;
                    case '5': rank = Rank.Five; break;
                    case '6': rank = Rank.Six; break;
                    case '7': rank = Rank.Seven; break;
                    case '8': rank = Rank.Eight; break;
                    case '9': rank = Rank.Nine; break;
                    case 'J': rank = Rank.Jack; break;
                    case 'Q': rank = Rank.Queen; break;
                    case 'K': rank = Rank.King; break;
                    case 'A': rank = Rank.Ace; break;
                    default: throw new FormatException("Invalid first character: rank expected.");
                }
            }
            else if (s.Length == 3) {
                if (chars[0] == '1' && chars[1] == '0') {
                    rank = Rank.Ten;
                }
                else {
                    throw new FormatException("Card represented by 3 symbols can only start with '10'.");
                }
            }
            else {
                throw new FormatException("A card is represented by 2 or 3 symbols.");
            }

            Suit suit;
            switch (chars[s.Length - 1]) {
                case 'H': suit = Suit.Heart; break;
                case 'D': suit = Suit.Diamond; break;
                case 'S': suit = Suit.Spade; break;
                case 'C': suit = Suit.Club; break;
                default: throw new FormatException("Invalid suit: 'H', 'D', 'S' or 'C' expected.");
            }

            return new Card(rank, suit);
        }
        
        public override string ToString() {
            var sb = new StringBuilder();

            switch (Rank) {
                case Rank.Two: sb.Append("2"); break;
                case Rank.Three: sb.Append("3"); break;
                case Rank.Four: sb.Append("4"); break;
                case Rank.Five: sb.Append("5"); break;
                case Rank.Six: sb.Append("6"); break;
                case Rank.Seven: sb.Append("7"); break;
                case Rank.Eight: sb.Append("8"); break;
                case Rank.Nine: sb.Append("9"); break;
                case Rank.Ten: sb.Append("10"); break;
                case Rank.Jack: sb.Append("J"); break;
                case Rank.Queen: sb.Append("Q"); break;
                case Rank.King: sb.Append("K"); break;
                case Rank.Ace: sb.Append("A"); break;
            }

            switch (Suit) {
                case Suit.Heart: sb.Append("H"); break;
                case Suit.Diamond: sb.Append("D"); break;
                case Suit.Spade: sb.Append("S"); break;
                case Suit.Club: sb.Append("C"); break;
            }

            return sb.ToString();
        }
    }
}
