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

        public void ProcessRequest() => Casino.GetTablePlayer(_client.Username).TableController.PlayerRaise(_raisedToAmount);
    }
}