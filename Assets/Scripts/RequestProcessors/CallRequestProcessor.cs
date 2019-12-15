using System.IO;
using Poker;

namespace RequestProcessors {
    public class CallRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            int callAmount = 139; //TODO call amount should be read from round data

            Table table = Casino.GetTablePlayer(username).Table;
            table.Broadcast(ServerResponse.PlayerCalled);
            table.Broadcast(table.GetIndexOf(username).ToString());
            table.Broadcast(callAmount.ToString());
        }
    }
}