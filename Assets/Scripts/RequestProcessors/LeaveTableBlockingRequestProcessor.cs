using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class LeaveTableBlockingRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        
        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            var tablePlayer = Casino.GetTablePlayer(_client.Username);
            Casino.RemoveTablePlayer(tablePlayer);
            Casino.AddLobbyPlayer(new LobbyPlayer(_client, tablePlayer.ChipCount));
        }
    }
}