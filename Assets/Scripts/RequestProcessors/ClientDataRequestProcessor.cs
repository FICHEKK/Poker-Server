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
            _requestedUsername = client.Reader.ReadLine();
        }

        public void ProcessRequest()
        {
            _client.Writer.WriteLine(DaoProvider.Dao.GetChipCount(_requestedUsername).ToString());
            _client.Writer.WriteLine(DaoProvider.Dao.GetWinCount(_requestedUsername).ToString());
            _client.Writer.WriteLine(DaoProvider.Dao.GetEloRating(_requestedUsername).ToString());
        }
    }
}