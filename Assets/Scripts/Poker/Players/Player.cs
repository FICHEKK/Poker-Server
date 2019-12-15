using System.IO;

namespace Poker.Players {
    
    /// <summary>
    /// Abstract implementation of a player.
    /// </summary>
    public abstract class Player {
        
        /// <summary>
        /// The username of this player.
        /// </summary>
        public string Username { get; }
        
        /// <summary>
        /// The chip count of this player.
        /// </summary>
        public int ChipCount { get; set; }

        /// <summary>
        /// The reader used to read data from this player.
        /// </summary>
        public StreamReader Reader { get; }
        
        /// <summary>
        /// The writer used to write data to this player.
        /// </summary>
        public StreamWriter Writer { get; }

        /// <summary>
        /// Constructs a new player.
        /// </summary>
        protected Player(string username, int chipCount, StreamReader reader, StreamWriter writer) {
            Username = username;
            ChipCount = chipCount;
            Reader = reader;
            Writer = writer;
        }

        /// <summary>
        /// Players are equal if they have the same username.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if the given object is equal to this player.</returns>
        public override bool Equals(object obj) {
            if (!(obj is Player player)) return false;
            return Username == player.Username;
        }

        protected bool Equals(Player other) {
            return Username == other.Username;
        }

        public override int GetHashCode() {
            return Username != null ? Username.GetHashCode() : 0;
        }
    }
}