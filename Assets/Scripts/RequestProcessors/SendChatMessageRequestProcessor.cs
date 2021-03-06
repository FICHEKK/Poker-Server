using Poker;

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
            _message = _client.ReadLine();
        }

        public void ProcessRequest()
        {
            var player = Casino.GetTablePlayer(_client.Username);
            player.TableController.PlayerSendChatMessage(player.Index, _message);
        }
    }
}