using System.IO;
using Poker;

namespace RequestProcessors {
    public class CheckRequestProcessor : IRequestProcessor {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer) {
            Dealer dealer = Casino.GetTablePlayer(username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerChecked);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            
            dealer.Round.PlayerChecked();
        }
    }
}