using Poker;

namespace RequestProcessors
{
    public class TableListRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            client.Writer.WriteLine(Casino.TableCount);

            foreach (string tableName in Casino.TableNames)
            {
                Table table = Casino.GetTable(tableName);

                client.Writer.WriteLine(tableName);
                client.Writer.WriteLine(table.SmallBlind);
                client.Writer.WriteLine(table.PlayerCount);
                client.Writer.WriteLine(table.MaxPlayers);
            }
        }
    }
}