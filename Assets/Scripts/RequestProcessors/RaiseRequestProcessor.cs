using System.IO;
using Poker;

namespace RequestProcessors {
    public class RaiseRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            int raiseAmount = int.Parse(reader.ReadLine());

            Table table = Casino.GetTablePlayer(username).Table;
            table.Broadcast(ServerResponse.PlayerRaised);
            table.Broadcast(table.GetIndexOf(username).ToString());
            table.Broadcast(raiseAmount.ToString());
        }
    }
}