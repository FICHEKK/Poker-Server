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
            package.Append(Casino.TableCount);

            foreach (var tableName in Casino.TableNames)
            {
                var tableContext = Casino.GetTableController(tableName);

                package.Append(tableName);
                package.Append(tableContext.SmallBlind);
                package.Append(tableContext.PlayerCount);
                package.Append(tableContext.MaxPlayers);
                package.Append(tableContext.IsRanked);
                package.Append(tableContext.IsLocked);
            }
            
            package.Send();
        }
    }
}