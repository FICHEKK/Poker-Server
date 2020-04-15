using Poker;

namespace RequestProcessors
{
    public class RaiseRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => true;
        private Client _client;
        private int _raisedToAmount;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _raisedToAmount = int.Parse(_client.ReadLine());
        }

        public void ProcessRequest()
        {
            var player = Casino.GetTablePlayer(_client.Username);
            var package = new Client.Package(player.Table.GetActiveClients());
            package.Append(ServerResponse.PlayerRaised);
            package.Append(player.Index);
            package.Append(_raisedToAmount);
            package.Send();

            player.Table.Dealer.Round.PlayerRaised(_raisedToAmount);
        }
    }
}