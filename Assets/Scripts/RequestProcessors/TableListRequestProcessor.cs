using Poker;

namespace RequestProcessors
{
    public class TableListRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;

        public void ReadPayloadData(Client client)
        {
            _client = client;
        }

        public void ProcessRequest()
        {
            var package = new Client.Package(_client);
            package.Append(Casino.TableControllers.Count);

            foreach (var tableController in Casino.TableControllers.Values)
            {
                package.Append(tableController.Title)
                       .Append(tableController.SmallBlind)
                       .Append(tableController.PlayerCount)
                       .Append(tableController.MaxPlayers)
                       .Append(tableController.IsRanked)
                       .Append(tableController.IsLocked);
            }
            
            package.Send();
        }
    }
}