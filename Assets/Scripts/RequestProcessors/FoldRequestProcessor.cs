using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class FoldRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => true;
        private Client _client;

        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            TablePlayer player = Casino.GetTablePlayer(_client.Username);
            Dealer dealer = player.Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerFolded);
            dealer.Broadcast(player.Index.ToString());

            dealer.Round.PlayerFolded();
        }
    }
}