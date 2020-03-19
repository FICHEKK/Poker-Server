using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class DisconnectRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            if (client.IsLoggedIn)
            {
                LobbyPlayer lobbyPlayer = Casino.GetLobbyPlayer(client.Username);
                if (lobbyPlayer != null) Casino.RemoveLobbyPlayer(lobbyPlayer);

                TablePlayer tablePlayer = Casino.GetTablePlayer(client.Username);
                if (tablePlayer != null) Casino.RemoveTablePlayer(tablePlayer);
            }

            Server.DisconnectClient(client.Identifier);
        }
    }
}