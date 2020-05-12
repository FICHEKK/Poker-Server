using Dao;

namespace RequestProcessors
{
    public class ClientDataRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        private string _requestedUsername;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _requestedUsername = _client.ReadLine();
        }

        public void ProcessRequest()
        {
            new Client.Package(_client)
                .Append(DaoProvider.Dao.GetChipCount(_requestedUsername))
                .Append(DaoProvider.Dao.GetWinCount(_requestedUsername))
                .Append(DaoProvider.Dao.GetEloRating(_requestedUsername))
                .Send();
        }
    }
}