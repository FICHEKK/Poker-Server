using System.IO;
using Dao;
using Poker;

namespace RequestProcessors {
    public class LoginRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            string password = reader.ReadLine();

            if (!DaoProvider.Dao.IsRegistered(username)) {
                writer.BaseStream.WriteByte((byte) ServerLoginResponse.UsernameNotRegistered);
                return;
            }

            if (Casino.HasPlayerWithUsername(username)) {
                writer.BaseStream.WriteByte((byte) ServerLoginResponse.AlreadyLoggedIn);
                return;
            }

            if (!DaoProvider.Dao.Login(username, password)) {
                writer.BaseStream.WriteByte((byte) ServerLoginResponse.WrongPassword);
                return;
            }

            Casino.AddPlayer(username);
            writer.BaseStream.WriteByte((byte) ServerLoginResponse.Success);
        }
    }
}