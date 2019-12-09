namespace Poker.Cards {
    public class HandAnalyser {
    
        public HandValue HandValue { get; private set; }
        public int[] RankCounters { get; } = new int[13];

        private Card[] cards;

        private int _singles;
        private int _pairs;
        private int _threes;
        private int _fours;

        public HandAnalyser(Hand hand) {
            cards = hand.Cards;
            AnalyzeCards();
        }

        private void AnalyzeCards() {
            // Count the frequencies of card ranks.
            foreach (Card card in cards) {
                RankCounters[(int) card.Rank - 1]++;
            }
        
            // Count the number of single cards, pairs, three of a kinds and four
            // of a kinds.
            for (int i = 0; i < 13; i++) {
                if (RankCounters[i] == 1)
                    _singles++;
            
                else if (RankCounters[i] == 2)
                    _pairs++;
            
                else if (RankCounters[i] == 3)
                    _threes++;
            
                else if (RankCounters[i] == 4)
                    _fours++;
            }

            EvaluateHandValue();
        }

        private void EvaluateHandValue() {
            bool isStraight = IsStraight();
            bool isFlush = IsFlush();

            if (isFlush && isStraight) {
                if (Contains(Rank.Ace)) {
                    HandValue = HandValue.RoyalFlush;
                } else {
                    HandValue = HandValue.StraightFlush;
                }
            
            } else if (_fours == 1) {
                HandValue = HandValue.Fours;

            } else if (_threes == 1 && _pairs == 1) {
                HandValue = HandValue.FullHouse;

            } else if (isFlush) {
                HandValue = HandValue.Flush;

            } else if (isStraight) {
                HandValue = HandValue.Straight;

            } else if (_threes == 1 && _singles == 2) {
                HandValue = HandValue.Threes;

            } else if (_pairs == 2 && _singles == 1) {
                HandValue = HandValue.TwoPair;

            } else if (_pairs == 1 && _singles == 3) {
                HandValue = HandValue.OnePair;

            } else {
                HandValue = HandValue.HighCard;

            }
        }

        private bool IsStraight() {
            bool[] flags = new bool[14];

            // Fill in the flags based on the card ranks.
            foreach (Card card in cards) {
                if (card.Rank == Rank.Ace) {
                    flags[0] = true;
                    flags[13] = true;
                } else {
                    flags[(int) card.Rank] = true;
                }
            }

            int streak = 0;

            for (int i = 0; i < 14; i++) {
                if (flags[i]) {
                    if (++streak == 5)
                        return true;
                } else {
                    streak = 0;
                }
            }

            return false;
        }

        private bool IsFlush() {
            Suit suit = cards[0].Suit;

            for (int i = 1; i < 5; i++) {
                if (cards[i].Suit != suit)
                    return false;
            }

            return true;
        }

        private bool Contains(Rank rank) {
            foreach (Card card in cards) {
                if (card.Rank == rank)
                    return true;
            }

            return false;
        }
    }
}