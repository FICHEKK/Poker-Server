using System.IO;
using Poker;

namespace RequestProcessors
{
    public class LogoutRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            Casino.RemoveLobbyPlayer(Casino.GetLobbyPlayer(username));
        }
    }
}