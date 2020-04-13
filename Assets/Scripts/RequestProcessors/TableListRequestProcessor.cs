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
            _client.Writer.WriteLine(Casino.TableCount);

            foreach (string tableName in Casino.TableNames)
            {
                Table table = Casino.GetTable(tableName);

                _client.Writer.WriteLine(tableName);
                _client.Writer.WriteLine(table.SmallBlind);
                _client.Writer.WriteLine(table.PlayerCount);
                _client.Writer.WriteLine(table.MaxPlayers);
            }
        }
    }
}