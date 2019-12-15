using System.IO;
using Poker;

namespace RequestProcessors {
    public class CheckRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();

            Table table = Casino.GetTablePlayer(username).Table;
            table.Broadcast(ServerResponse.PlayerChecked);
            table.Broadcast(table.GetIndexOf(username).ToString());
        }
    }
}