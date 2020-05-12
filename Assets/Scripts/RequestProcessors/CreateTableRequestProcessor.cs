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
        private bool _isRanked;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _tableTitle = _client.ReadLine();
            _smallBlind = int.Parse(_client.ReadLine());
            _maxPlayers = int.Parse(_client.ReadLine());
            _isRanked = _client.ReadLine() == "R";
        }

        public void ProcessRequest()
        {
            var package = new Client.Package(_client);
            
            if (Casino.HasTableWithTitle(_tableTitle))
            {
                package.Append(ServerResponse.CreateTableTitleTaken);
            }
            else
            {
                if (_isRanked)
                {
                    Casino.AddTableController(new RankedTableController(new Table(_maxPlayers), _tableTitle, _smallBlind));
                }
                else
                {
                    Casino.AddTableController(new StandardTableController(new Table(_maxPlayers), _tableTitle, _smallBlind));
                }
                
                package.Append(ServerResponse.CreateTableSuccess);
            }
            
            package.Send();
        }
    }
}