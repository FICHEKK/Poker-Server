using Poker;

namespace RequestProcessors
{
    public class CreateTableRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            string tableTitle = client.Reader.ReadLine();
            int smallBlind = int.Parse(client.Reader.ReadLine());
            int maxPlayers = int.Parse(client.Reader.ReadLine());

            if (Casino.HasTableWithTitle(tableTitle))
            {
                client.Writer.BaseStream.WriteByte((byte) ServerCreateTableResponse.TitleTaken);
                return;
            }

            Casino.AddTable(new Table(tableTitle, smallBlind, maxPlayers));
            client.Writer.BaseStream.WriteByte((byte) ServerCreateTableResponse.Success);
        }
    }
}