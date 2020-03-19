using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class LeaveTableRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            TablePlayer tablePlayer = Casino.GetTablePlayer(client.Username);
            Casino.RemoveTablePlayer(tablePlayer);

            LobbyPlayer lobbyPlayer = new LobbyPlayer(client.Username, tablePlayer.ChipCount, tablePlayer.Reader, tablePlayer.Writer);
            Casino.AddLobbyPlayer(lobbyPlayer);
            
            client.Writer.BaseStream.WriteByte((byte) ServerResponse.LeaveTableSuccess);
        }
    }
}