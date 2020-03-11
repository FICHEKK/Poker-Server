using System.IO;
using Dao;

namespace RequestProcessors
{
    public class ClientDataRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            string requestedUsername = reader.ReadLine();

            writer.WriteLine(DaoProvider.Dao.GetChipCount(requestedUsername).ToString());
            writer.WriteLine(DaoProvider.Dao.GetWinCount(requestedUsername).ToString());
        }
    }
}