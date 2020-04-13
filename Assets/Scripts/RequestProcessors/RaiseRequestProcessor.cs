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
            _raisedToAmount = int.Parse(client.Reader.ReadLine());
        }

        public void ProcessRequest()
        {
            Dealer dealer = Casino.GetTablePlayer(_client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerRaised);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(_client.Username).ToString());
            dealer.Broadcast(_raisedToAmount);

            dealer.Round.PlayerRaised(_raisedToAmount);
        }
    }
}