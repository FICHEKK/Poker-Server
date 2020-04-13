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
            _callAmount = int.Parse(client.Reader.ReadLine());
        }

        public void ProcessRequest()
        {
            Dealer dealer = Casino.GetTablePlayer(_client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerCalled);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(_client.Username).ToString());
            dealer.Broadcast(_callAmount);

            dealer.Round.PlayerCalled(_callAmount);
        }
    }
}