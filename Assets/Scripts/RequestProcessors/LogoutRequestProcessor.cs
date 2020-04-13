using Poker;

namespace RequestProcessors
{
    public class LogoutRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;

        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            Casino.RemoveLobbyPlayer(Casino.GetLobbyPlayer(_client.Username));
            _client.IsLoggedIn = false;
        }
    }
}