using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class FoldRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            TablePlayer player = Casino.GetTablePlayer(client.Username);
            Dealer dealer = player.Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerFolded);
            dealer.Broadcast(player.Index.ToString());

            dealer.Round.PlayerFolded();
        }
    }
}