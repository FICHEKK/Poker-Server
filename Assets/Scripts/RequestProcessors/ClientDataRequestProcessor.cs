using Dao;

namespace RequestProcessors
{
    public class ClientDataRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            string requestedUsername = client.Reader.ReadLine();

            client.Writer.WriteLine(DaoProvider.Dao.GetChipCount(requestedUsername).ToString());
            client.Writer.WriteLine(DaoProvider.Dao.GetWinCount(requestedUsername).ToString());
            client.Writer.WriteLine(DaoProvider.Dao.GetEloRating(requestedUsername).ToString());
        }
    }
}