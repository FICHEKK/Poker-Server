using System.IO;
using Poker;

namespace RequestProcessors {
    public class AllInRequestProcessor : IRequestProcessor{
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            int allInAmount = 5000; //TODO should be read from table round data

            Table table = Casino.GetTablePlayer(username).Table;
            table.Broadcast(ServerResponse.PlayerAllIn);
            table.Broadcast(table.GetIndexOf(username).ToString());
            table.Broadcast(allInAmount.ToString());
        }
    }
}