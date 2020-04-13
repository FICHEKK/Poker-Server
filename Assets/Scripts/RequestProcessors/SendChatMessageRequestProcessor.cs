using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class SendChatMessageRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        private string _message;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _message = client.Reader.ReadLine();
        }

        public void ProcessRequest()
        {
            TablePlayer player = Casino.GetTablePlayer(_client.Username);
            Dealer dealer = player.Table.Dealer;
            dealer.Broadcast(ServerResponse.ChatMessage);
            dealer.Broadcast(player.Index.ToString());
            dealer.Broadcast(_message);
        }
    }
}