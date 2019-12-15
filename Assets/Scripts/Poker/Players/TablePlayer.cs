using System.IO;

namespace Poker.Players {
    
    /// <summary>
    /// Models a player that is currently on a table.
    /// </summary>
    public class TablePlayer : Player {
        
        /// <summary>
        /// The table that this player is currently on.
        /// </summary>
        public Table Table { get; set; }
        
        /// <summary>
        /// The amount of chips that this player currently has at the table.
        /// </summary>
        public int Stack { get; set; }

        public TablePlayer(string username, int chipCount, Table table, int buyIn, StreamReader reader, StreamWriter writer)
            : base(username, chipCount, reader, writer) {
            Table = table;
            Stack = buyIn;
        }
        
        /// <summary>
        /// Leaves the current table, unless this player is currently not on any table.
        /// </summary>
        /// <returns></returns>
        public bool LeaveTable() {
            if (Table == null) return false;
            
            Table.RemovePlayer(this);
            Table = null;
            return true;
        }
    }
}