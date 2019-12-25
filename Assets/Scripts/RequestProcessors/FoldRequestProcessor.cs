using System.IO;
using Poker;
using Poker.Players;

namespace RequestProcessors {
    public class FoldRequestProcessor : IRequestProcessor {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer) {
            TablePlayer player = Casino.GetTablePlayer(username);
            Dealer dealer = player.Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerFolded);
            dealer.Broadcast(player.Index.ToString());
            
            dealer.Round.PlayerFolded();
        }
    }
}