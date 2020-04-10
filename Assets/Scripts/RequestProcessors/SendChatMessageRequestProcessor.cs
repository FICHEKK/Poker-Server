using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class SendChatMessageRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            TablePlayer player = Casino.GetTablePlayer(client.Username);
            Dealer dealer = player.Table.Dealer;
            dealer.Broadcast(ServerResponse.ChatMessage);
            dealer.Broadcast(player.Index.ToString());
            
            string message = client.Reader.ReadLine();
            dealer.Broadcast(message);
        }
    }
}