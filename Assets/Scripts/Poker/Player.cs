using System.IO;

namespace Poker {
    public class Player {
        public string Username { get; }
        public int ChipCount { get; }
        
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }

        public Player(string username, int chipCount, StreamReader reader, StreamWriter writer) {
            Username = username;
            ChipCount = chipCount;
            Reader = reader;
            Writer = writer;
        }
    }
}