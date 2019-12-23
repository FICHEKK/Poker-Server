using System.IO;

namespace Poker.Players {
    
    /// <summary> Models a player that is currently in lobby. </summary>
    public class LobbyPlayer : Player {
        
        /// <summary> Constructs a new lobby player. </summary>
        /// <param name="username"> The player's username. </param>
        /// <param name="chipCount"> The player's chip count. </param>
        /// <param name="reader"> The player's reader. </param>
        /// <param name="writer"> The player's writer. </param>
        public LobbyPlayer(string username, int chipCount, StreamReader reader, StreamWriter writer)
            : base(username, chipCount, reader, writer) { }
    }
}