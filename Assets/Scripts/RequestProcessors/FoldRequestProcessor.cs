using System.IO;
using Poker;

namespace RequestProcessors {
    public class FoldRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();

            Dealer dealer = Casino.GetTablePlayer(username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerFolded);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(username).ToString());
            
            dealer.Round.PlayerFolded();
        }
    }
}