using Poker;

namespace RequestProcessors
{
    public class CallRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => true;
        private Client _client;
        private int _callAmount;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _callAmount = int.Parse(client.ReadLine());
        }

        public void ProcessRequest()
        {
            var player = Casino.GetTablePlayer(_client.Username);
            var package = new Client.Package(player.Table.GetActiveClients());
            package.Append(ServerResponse.PlayerCalled);
            package.Append(player.Index);
            package.Append(_callAmount);
            package.Send();

            player.Table.Dealer.Round.PlayerCalled(_callAmount);
        }
    }
}