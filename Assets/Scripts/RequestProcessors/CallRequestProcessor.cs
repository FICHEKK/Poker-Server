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

        public void ProcessRequest() => Casino.GetTablePlayer(_client.Username).TableController.PlayerCall(_callAmount);
    }
}