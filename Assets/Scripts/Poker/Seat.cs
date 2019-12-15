using Poker.Players;

namespace Poker {
    
    /// <summary>
    /// Models a single spot on the poker table.
    /// </summary>
    public class Seat {
        
        /// <summary>
        /// This seat's index (the unique position on the table).
        /// </summary>
        public int Index { get; }
        
        /// <summary>
        /// The player currently occupying this seat.
        /// </summary>
        public TablePlayer Player { get; set; }
        
        /// <summary>
        /// The amount of chips that player on this seat has.
        /// </summary>
        public int ChipCount { get; set; }

        /// <summary>
        /// Checks if this seat is currently empty (has no player occupying it).
        /// </summary>
        public bool IsEmpty => Player == null;

        /// <summary>
        /// Checks if this seat is currently occupied by a player.
        /// </summary>
        public bool IsOccupied => Player != null;

        /// <summary>
        /// Constructs a new seat with the given index.
        /// </summary>
        /// <param name="index">Index of the seat.</param>
        public Seat(int index) {
            Index = index;
        }
    }
}