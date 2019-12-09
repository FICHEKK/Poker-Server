using System.IO;
using System.Net.Sockets;

namespace Poker {
    public class Player {
        public string Username { get; }
        public int ChipCount { get; }
        public int WinCount { get; }
        public TcpClient TcpClient { get; }
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }

        public Player(string username, int chipCount, int winCount, TcpClient client) {
            Username = username;
            ChipCount = chipCount;
            WinCount = winCount;
            TcpClient = client;
            Reader = new StreamReader(TcpClient.GetStream());
            Writer = new StreamWriter(TcpClient.GetStream());
        }
    }
}