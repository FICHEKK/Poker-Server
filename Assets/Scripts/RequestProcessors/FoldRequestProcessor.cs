using Poker;

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
            var player = Casino.GetTablePlayer(_client.Username);
            var package = new Client.Package(player.Table.GetActiveClients());
            package.Append(ServerResponse.PlayerFolded);
            package.Append(player.Index);
            package.Send();

            player.Table.Dealer.Round.PlayerFolded();
        }
    }
}