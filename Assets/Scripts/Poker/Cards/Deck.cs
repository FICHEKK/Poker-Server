using System;
using Random = System.Random;

namespace Poker.Cards {
    
    /// <summary>
    /// A simple 52 card deck implementation that offers basic
    /// deck operations such as shuffling and card retrieval.
    /// </summary>
    public class Deck {
        
        /// <summary> The random number generator used for deck shuffling. </summary>
        private static readonly Random RandomNumberGenerator = new Random();

        /// <summary> Index of the current top-card in the deck. </summary>
        private int _index;

        /// <summary> An array of 52 card references. </summary>
        private readonly Card[] _cards = new Card[52];

        /// <summary> Constructs a new 52 card deck. </summary>
        public Deck() {
            int i = 0;

            foreach (Suit suit in Enum.GetValues(typeof(Suit))) {
                foreach (Rank rank in Enum.GetValues(typeof(Rank))) {
                    _cards[i++] = new Card(rank, suit);
                }
            }
        }

        /// <summary>
        /// Randomly shuffles the cards in the deck. After shuffling,
        /// there will be 52 cards that can be retrieved (meaning that
        /// shuffling resets the number of cards in the deck).
        /// </summary>
        public void Shuffle() {
            int n = _cards.Length;

            while (n > 1) {
                int k = RandomNumberGenerator.Next(n--);

                Card temp = _cards[n];
                _cards[n] = _cards[k];
                _cards[k] = temp;
            }

            _index = 0;
        }

        /// <summary> Returns true if there are any cards in the deck left, false otherwise. </summary>
        public bool HasNextCard() {
            return _index < _cards.Length;
        }

        /// <summary>
        /// Grabs the next card from the deck and returns that card.
        /// If there are no more cards left, returns null.
        /// </summary>
        public Card GetNextCard() {
            return HasNextCard() ? _cards[_index++] : null;
        }
    }
}