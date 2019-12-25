using System.IO;
using Poker;
using Poker.Players;

namespace RequestProcessors {
    public class DisconnectRequestProcessor : IRequestProcessor {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer) {
            LobbyPlayer lobbyPlayer = Casino.GetLobbyPlayer(username);
            if (lobbyPlayer != null) {
                Casino.RemoveLobbyPlayer(lobbyPlayer);
                return;
            }

            TablePlayer tablePlayer = Casino.GetTablePlayer(username);
            if (tablePlayer != null) {
                Casino.RemoveTablePlayer(tablePlayer);
            }
        }
    }
}