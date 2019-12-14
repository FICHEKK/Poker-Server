using System.IO;
using Dao;

namespace RequestProcessors {
    public class ClientDataRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();

            writer.WriteLine(DaoProvider.Dao.GetChipCount(username).ToString());
            writer.WriteLine(DaoProvider.Dao.GetWinCount(username).ToString());
            writer.Flush();
        }
    }
}