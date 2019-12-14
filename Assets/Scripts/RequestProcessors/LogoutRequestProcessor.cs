using System.IO;
using Poker;

namespace RequestProcessors {
    public class LogoutRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            Casino.RemovePlayer(username);
        }
    }
}