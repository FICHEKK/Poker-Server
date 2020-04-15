using Poker;

namespace RequestProcessors
{
    public class AllInRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => true;
        private Client _client;
        private int _allInAmount;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _allInAmount = int.Parse(client.ReadLine());
        }

        public void ProcessRequest()
        {
            var player = Casino.GetTablePlayer(_client.Username);
            var package = new Client.Package(player.Table.GetActiveClients());
            package.Append(ServerResponse.PlayerAllIn);
            package.Append(player.Index);
            package.Append(_allInAmount);
            package.Send();

            player.Table.Dealer.Round.PlayerAllIn(_allInAmount);
        }
    }
}