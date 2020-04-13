using Poker;

namespace RequestProcessors
{
    public class CheckRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => true;
        private Client _client;

        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            Dealer dealer = Casino.GetTablePlayer(_client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerChecked);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(_client.Username).ToString());

            dealer.Round.PlayerChecked();
        }
    }
}