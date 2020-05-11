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

            foreach (string tableName in Casino.TableNames)
            {
                var table = Casino.GetTable(tableName);

                package.Append(tableName);
                package.Append(table.SmallBlind);
                package.Append(table.PlayerCount);
                package.Append(table.MaxPlayers);
                package.Append(table.IsRanked);
                package.Append(table.IsLocked);
            }
            
            package.Send();
        }
    }
}