using System.IO;
using Poker;

namespace RequestProcessors {
    public class FoldRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();

            Table table = Casino.GetTablePlayer(username).Table;
            table.Broadcast(ServerResponse.PlayerFolded);
            table.Broadcast(table.GetIndexOf(username).ToString());
        }
    }
}