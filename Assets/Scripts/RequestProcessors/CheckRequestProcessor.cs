using System.IO;
using Poker;

namespace RequestProcessors {
    public class CheckRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            
            Dealer dealer = Casino.GetTablePlayer(username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerChecked);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            
            dealer.Round.PlayerChecked();
        }
    }
}