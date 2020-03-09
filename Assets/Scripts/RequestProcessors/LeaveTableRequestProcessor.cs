using System.IO;
using Poker;

namespace RequestProcessors
{
    public class LeaveTableRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            Casino.MovePlayerFromTableToLobby(username);
            writer.BaseStream.WriteByte((byte) ServerResponse.LeaveTableSuccess);
        }
    }
}