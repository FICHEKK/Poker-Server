using Poker;

namespace RequestProcessors
{
    public class LeaveTableRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        
        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            var player = Casino.GetTablePlayer(_client.Username);
            player.TableController.PlayerLeave(player);
        }
    }
}