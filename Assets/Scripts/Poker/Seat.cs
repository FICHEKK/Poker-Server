using Poker.Players;

namespace Poker {
    
    /// <summary>
    /// Models a single spot on the poker table.
    /// </summary>
    public class Seat {
        
        /// <summary>
        /// The table this seat is on.
        /// </summary>
        public Table Table { get; }
        
        /// <summary>
        /// This seat's index (the unique position on the table).
        /// </summary>
        public int Index { get; }
        
        /// <summary>
        /// The player currently occupying this seat.
        /// </summary>
        public TablePlayer Player { get; set; }

        /// <summary>
        /// Checks if this seat is currently occupied by a player.
        /// </summary>
        public bool IsOccupied => Player != null;

        /// <summary>
        /// Constructs a new seat with the given index.
        /// </summary>
        /// <param name="table">The table that this seat will be on.</param>
        /// <param name="index">Index of the seat.</param>
        public Seat(Table table, int index) {
            Table = table;
            Index = index;
        }
    }
}