using System.IO;
using Dao;

namespace RequestProcessors {
    public class LoginRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            string password = reader.ReadLine();

            if (!DaoProvider.Dao.IsRegistered(username)) {
                writer.BaseStream.WriteByte((byte) ServerResponse.LoginFailedUsernameNotRegistered);
                return;
            }

            bool loginSucceeded = DaoProvider.Dao.Login(username, password);

            if (loginSucceeded) {
                writer.BaseStream.WriteByte((byte) ServerResponse.LoginSucceeded);
                writer.WriteLine(DaoProvider.Dao.GetChipCount(username).ToString());
                writer.WriteLine(DaoProvider.Dao.GetWinCount(username).ToString());
                writer.Flush();
            }
            else {
                writer.BaseStream.WriteByte((byte) ServerResponse.LoginFailedWrongPassword);
            }
        }
    }
}