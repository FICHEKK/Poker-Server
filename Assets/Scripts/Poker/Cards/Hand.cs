using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poker.Cards
{
    public class Hand : IComparable<Hand>
    {
        public Card[] Cards { get; } = new Card[5];
        public HandAnalyser HandAnalyser { get; }

        public Hand(Card c0, Card c1, Card c2, Card c3, Card c4)
        {
            Cards[0] = c0;
            Cards[1] = c1;
            Cards[2] = c2;
            Cards[3] = c3;
            Cards[4] = c4;

            Array.Sort(Cards);

            HandAnalyser = new HandAnalyser(this);
        }

        public int CompareTo(Hand other)
        {
            HandValue handValue = HandAnalyser.HandValue;
            HandValue handValueOther = other.HandAnalyser.HandValue;

            if ((int) handValue > (int) handValueOther) return 1;
            if ((int) handValue < (int) handValueOther) return -1;

            if (handValue == HandValue.HighCard) return CompareHighCard(other);
            if (handValue == HandValue.OnePair) return CompareOnePair(other);
            if (handValue == HandValue.TwoPair) return CompareTwoPair(other);
            if (handValue == HandValue.Threes) return CompareThrees(other);
            if (handValue == HandValue.Straight) return CompareStraight(other);
            if (handValue == HandValue.Flush) return CompareFlush(other);
            if (handValue == HandValue.FullHouse) return CompareFullHouse(other);
            if (handValue == HandValue.Fours) return CompareFours(other);
            if (handValue == HandValue.StraightFlush) return CompareStraightFlush(other);

            return 0;
        }

        private int CompareHighCard(Hand other)
        {
            return CompareMultiple(other, 1);
        }

        private int CompareOnePair(Hand other)
        {
            int result = CompareSingle(other, 2);
            return result != 0 ? result : CompareMultiple(other, 1);
        }

        private int CompareTwoPair(Hand other)
        {
            int result = CompareMultiple(other, 2);
            return result != 0 ? result : CompareSingle(other, 1);
        }

        private int CompareThrees(Hand other)
        {
            int result = CompareSingle(other, 3);
            return result != 0 ? result : CompareMultiple(other, 1);
        }

        private int CompareStraight(Hand other)
        {
            return CompareHighCard(other);
        }

        private int CompareFlush(Hand other)
        {
            return CompareHighCard(other);
        }

        private int CompareFullHouse(Hand other)
        {
            int result = CompareSingle(other, 3);
            return result != 0 ? result : CompareSingle(other, 2);
        }

        private int CompareFours(Hand other)
        {
            int result = CompareSingle(other, 4);
            return result != 0 ? result : CompareSingle(other, 1);
        }

        private int CompareStraightFlush(Hand other)
        {
            return CompareHighCard(other);
        }

        private static int FindIndexOfRank(Hand hand, int cardinality)
        {
            int[] rankCounters = hand.HandAnalyser.RankCounters;
            for (int i = 0; i < rankCounters.Length; i++)
            {
                if (rankCounters[i] == cardinality)
                {
                    return i;
                }
            }

            return -1;
        }

        private static List<int> FindAllIndexesOfRank(Hand hand, int cardinality)
        {
            var indexes = new List<int>();

            int[] rankCounters = hand.HandAnalyser.RankCounters;
            for (int i = 0; i < rankCounters.Length; i++)
            {
                if (rankCounters[i] == cardinality)
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }

        private int CompareSingle(Hand other, int cardinality)
        {
            int rank = FindIndexOfRank(this, cardinality);
            int rankOther = FindIndexOfRank(other, cardinality);

            if (rank > rankOther) return 1;
            if (rank == rankOther) return 0;
            return -1;
        }

        private int CompareMultiple(Hand other, int cardinality)
        {
            List<int> ranks = FindAllIndexesOfRank(this, cardinality);
            List<int> ranksOther = FindAllIndexesOfRank(other, cardinality);

            ranks.Sort();
            ranksOther.Sort();

            for (int i = ranks.Count - 1; i >= 0; i--)
            {
                if (ranks[i] > ranksOther[i]) return 1;
                if (ranks[i] < ranksOther[i]) return -1;
            }

            return 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                sb.Append(Cards[i]).Append(" ");
            }

            sb.Append(HandAnalyser.HandValue);

            return sb.ToString();
        }

        public string ToStringPretty()
        {
            switch (HandAnalyser.HandValue)
            {
                case HandValue.HighCard:      return HighCardToString();
                case HandValue.OnePair:       return OnePairToString();
                case HandValue.TwoPair:       return TwoPairToString();
                case HandValue.Threes:        return ThreesToString();
                case HandValue.Straight:      return StraightToString(false);
                case HandValue.Flush:         return FlushToString();
                case HandValue.FullHouse:     return FullHouseToString();
                case HandValue.Fours:         return FoursToString();
                case HandValue.StraightFlush: return StraightToString(true);
                case HandValue.RoyalFlush:    return RoyalFlushToString();
                default: throw new ArgumentOutOfRangeException(nameof(HandAnalyser.HandValue), "Invalid hand value!");
            }
        }

        private string HighCardToString()
        {
            return "High card " + RankToString(Cards.Last().Rank);
        }

        private string OnePairToString()
        {
            var index = FindIndexOfRank(this, 2);
            return "One pair of " + IndexToString(index) + "'s";
        }

        private string TwoPairToString()
        {
            var indexes = FindAllIndexesOfRank(this, 2);
            return "Two pair " + IndexToString(indexes[0]) + "'s and " + IndexToString(indexes[1]) + "'s";
        }

        private string ThreesToString()
        {
            var index = FindIndexOfRank(this, 3);
            return "Three of a kind " + IndexToString(index) + "'s";
        }

        private string StraightToString(bool isFlush)
        {
            var first = Cards.First().Rank;
            var last = Cards.Last().Rank;
            
            // 2 3 4 5 A exception
            if (first == Rank.Two && last == Rank.Ace)
            {
                first = last;
                last = Cards[3].Rank;
            }
            
            return "Straight " + (isFlush ? "flush " : "") + RankToString(first) + " to " + RankToString(last) + (isFlush ? " !!" : "");
        }

        private string FlushToString()
        {
            return "Flush up to " + RankToString(Cards.Last().Rank);
        }

        private string FullHouseToString()
        {
            int threes = FindIndexOfRank(this, 3);
            int pair = FindIndexOfRank(this, 2);
            return "Full house " + IndexToString(threes) + "'s over " + IndexToString(pair) + "'s";
        }

        private string FoursToString()
        {
            var index = FindIndexOfRank(this, 4);
            return "Four of a kind " + IndexToString(index) + "'s !";
        }

        private string RoyalFlushToString()
        {
            return "Royal flush !!!";
        }

        private string IndexToString(int index)
        {
            return RankToString((Rank) (index + 1));
        }

        private string RankToString(Rank rank)
        {
            switch (rank)
            {
                case Rank.Two:   return "2";
                case Rank.Three: return "3";
                case Rank.Four:  return "4";
                case Rank.Five:  return "5";
                case Rank.Six:   return "6";
                case Rank.Seven: return "7";
                case Rank.Eight: return "8";
                case Rank.Nine:  return "9";
                case Rank.Ten:   return "10";
                case Rank.Jack:  return "J";
                case Rank.Queen: return "Q";
                case Rank.King:  return "K";
                case Rank.Ace:   return "A";
                default: throw new ArgumentOutOfRangeException(nameof(rank), rank, "Invalid rank value!");
            }
        }
    }
}