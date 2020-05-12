using Poker;

namespace RequestProcessors
{
    public class JoinTableRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        private string _tableTitle;
        private int _buyIn;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _tableTitle = _client.ReadLine();
            _buyIn = int.Parse(_client.ReadLine());
        }

        public void ProcessRequest()
        {
            var package = new Client.Package(_client);
            
            if (!Casino.HasTableWithTitle(_tableTitle))
            {
                package.Append(ServerResponse.JoinTableTableDoesNotExist);
                package.Send();
                return;
            }

            var tableController = Casino.GetTableController(_tableTitle);

            if (tableController.IsRanked && tableController.IsLocked)
            {
                package.Append(ServerResponse.JoinTableRankedMatchStarted);
                package.Send();
                return;
            }

            if (tableController.PlayerCount == tableController.MaxPlayers)
            {
                package.Append(ServerResponse.JoinTableTableFull);
                package.Send();
                return;
            }

            package.Append(ServerResponse.JoinTableSuccess);
            package.Send();
            
            tableController.PlayerJoin(_client, _buyIn);
        }
    }
}