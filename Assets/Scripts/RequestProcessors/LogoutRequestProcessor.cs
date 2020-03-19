using Poker;

namespace RequestProcessors
{
    public class LogoutRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            Casino.RemoveLobbyPlayer(Casino.GetLobbyPlayer(client.Username));
            client.IsLoggedIn = false;
        }
    }
}