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

        public void ProcessRequest() => Casino.GetTablePlayer(_client.Username).TableController.PlayerAllIn(_allInAmount);
    }
}