using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class DisconnectRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;

        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            if (_client.IsLoggedIn)
            {
                var lobbyPlayer = Casino.GetLobbyPlayer(_client.Username);
                if (lobbyPlayer != null) Casino.RemoveLobbyPlayer(lobbyPlayer);

                var tablePlayer = Casino.GetTablePlayer(_client.Username);
                if (tablePlayer != null) Casino.RemoveTablePlayer(tablePlayer);
            }

            Server.DisconnectClient(_client.Identifier);
        }
    }
}