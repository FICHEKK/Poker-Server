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
            TablePlayer tablePlayer = Casino.GetTablePlayer(_client.Username);
            Casino.RemoveTablePlayer(tablePlayer);

            LobbyPlayer lobbyPlayer = new LobbyPlayer(_client.Username, tablePlayer.ChipCount, tablePlayer.Reader, tablePlayer.Writer);
            Casino.AddLobbyPlayer(lobbyPlayer);
        }
    }
}