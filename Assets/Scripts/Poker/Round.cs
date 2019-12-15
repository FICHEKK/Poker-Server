using System;
using System.Collections.Generic;
using Poker.Cards;

namespace Poker {
    
    /// <summary>
    /// Holds data of a single poker round.
    /// </summary>
    public class Round {

        private const int MaxCommunityCardCount = 5;

        private Table _table;
        private List<Seat> _seats;
        private List<Card> _communityCards;

        public bool IsOver => _seats.Count == 1;

        /// <summary>
        /// Used to determine if everyone has performed the same action
        /// </summary>
        private int _actionCounter;

        /// <summary>
        /// Constructs a new poker round.
        /// </summary>
        /// <param name="table">The table that this round is being held on.</param>
        /// <param name="seats">Seats on table that participate in this round.</param>
        public Round(Table table, List<Seat> seats) {
            _table = table;
            _seats = seats;
        }

        public void AddCommunityCard(Card card) {
            if (_communityCards == null) {
                _communityCards = new List<Card>();
            }

            if (_communityCards.Count == MaxCommunityCardCount) {
                throw new IndexOutOfRangeException("Maximum number of community cards already reached.");
            }
            
            _communityCards.Add(card);
        }

        public bool RemoveFromPlay(Seat seat) {
            return _seats.Remove(seat);
        }
    }
}