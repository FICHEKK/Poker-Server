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
            _allInAmount = int.Parse(client.Reader.ReadLine());
        }

        public void ProcessRequest()
        {
            Dealer dealer = Casino.GetTablePlayer(_client.Username).Table.Dealer;
            dealer.Broadcast(ServerResponse.PlayerAllIn);
            dealer.Broadcast(dealer.Table.GetPlayerIndex(_client.Username).ToString());
            dealer.Broadcast(_allInAmount);

            dealer.Round.PlayerAllIn(_allInAmount);
        }
    }
}