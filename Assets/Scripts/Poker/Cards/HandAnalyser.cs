using System.Linq;

namespace Poker.Cards
{
    public class HandAnalyser
    {
        public HandValue HandValue { get; private set; }
        public int[] RankCounters { get; } = new int[13];

        private readonly Card[] _cards;

        private int _singles;
        private int _pairs;
        private int _threes;
        private int _fours;

        public HandAnalyser(Hand hand)
        {
            _cards = hand.Cards;
            AnalyzeCards();
        }

        private void AnalyzeCards()
        {
            // Count the frequencies of card ranks.
            foreach (Card card in _cards)
            {
                RankCounters[(int) card.Rank - 1]++;
            }

            // Count the number of single cards, pairs, three of a kinds and four
            // of a kinds.
            for (int i = 0; i < 13; i++)
            {
                switch (RankCounters[i])
                {
                    case 1:
                        _singles++;
                        break;
                    case 2:
                        _pairs++;
                        break;
                    case 3:
                        _threes++;
                        break;
                    case 4:
                        _fours++;
                        break;
                }
            }

            EvaluateHandValue();
        }

        private void EvaluateHandValue()
        {
            bool isStraight = IsStraight();
            bool isFlush = IsFlush();

            if (isFlush && isStraight)
            {
                HandValue = ContainsRank(Rank.Ace) && ContainsRank(Rank.Ten) ? HandValue.RoyalFlush : HandValue.StraightFlush;
            }
            else if (_fours == 1)
            {
                HandValue = HandValue.Fours;
            }
            else if (_threes == 1 && _pairs == 1)
            {
                HandValue = HandValue.FullHouse;
            }
            else if (isFlush)
            {
                HandValue = HandValue.Flush;
            }
            else if (isStraight)
            {
                HandValue = HandValue.Straight;
            }
            else if (_threes == 1 && _singles == 2)
            {
                HandValue = HandValue.Threes;
            }
            else if (_pairs == 2 && _singles == 1)
            {
                HandValue = HandValue.TwoPair;
            }
            else if (_pairs == 1 && _singles == 3)
            {
                HandValue = HandValue.OnePair;
            }
            else
            {
                HandValue = HandValue.HighCard;
            }
        }

        private bool IsStraight()
        {
            bool[] flags = new bool[14];

            // Fill in the flags based on the card ranks.
            foreach (Card card in _cards)
            {
                if (card.Rank == Rank.Ace)
                {
                    flags[0] = true;
                    flags[13] = true;
                }
                else
                {
                    flags[(int) card.Rank] = true;
                }
            }

            int streak = 0;

            for (int i = 0; i < 14; i++)
            {
                if (flags[i])
                {
                    if (++streak == 5) return true;
                }
                else
                {
                    streak = 0;
                }
            }

            return false;
        }

        private bool IsFlush() => _cards.All(card => card.Suit == _cards[0].Suit);
        private bool ContainsRank(Rank rank) => _cards.Any(card => card.Rank == rank);
    }
}