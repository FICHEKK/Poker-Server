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
            var package = new Client.Package(_client);
            package.Append(DaoProvider.Dao.GetChipCount(_requestedUsername));
            package.Append(DaoProvider.Dao.GetWinCount(_requestedUsername));
            package.Append(DaoProvider.Dao.GetEloRating(_requestedUsername));
            package.Send();
        }
    }
}