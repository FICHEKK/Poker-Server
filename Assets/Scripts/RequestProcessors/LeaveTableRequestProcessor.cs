using Poker;

namespace RequestProcessors
{
    public class LeaveTableRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        
        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            // Execute the potentially blocking part of this request on the table thread.
            var blockingRequestProcessor = new LeaveTableBlockingRequestProcessor();
            blockingRequestProcessor.ReadPayloadData(_client);
            Casino.GetTablePlayer(_client.Username).TableController.RequestProcessors.Add(blockingRequestProcessor);

            var player = Casino.GetTablePlayer(_client.Username);
            player.TableController.PlayerLeave(player);
        }
    }
}