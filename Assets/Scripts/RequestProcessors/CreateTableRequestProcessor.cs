using Poker;

namespace RequestProcessors
{
    public class CreateTableRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        private string _tableTitle;
        private int _smallBlind;
        private int _maxPlayers;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _tableTitle = client.Reader.ReadLine();
            _smallBlind = int.Parse(client.Reader.ReadLine());
            _maxPlayers = int.Parse(client.Reader.ReadLine());
        }

        public void ProcessRequest()
        {
            if (Casino.HasTableWithTitle(_tableTitle))
            {
                _client.Writer.BaseStream.WriteByte((byte) ServerCreateTableResponse.TitleTaken);
                return;
            }

            Casino.AddTable(new Table(_tableTitle, _smallBlind, _maxPlayers));
            _client.Writer.BaseStream.WriteByte((byte) ServerCreateTableResponse.Success);
        }
    }
}