using System.IO;
using Dao;
using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class LoginRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            string password = reader.ReadLine();

            if (!DaoProvider.Dao.IsRegistered(username))
            {
                writer.BaseStream.WriteByte((byte) ServerLoginResponse.UsernameNotRegistered);
                return;
            }

            if (DaoProvider.Dao.IsBanned(username))
            {
                writer.BaseStream.WriteByte((byte) ServerLoginResponse.UsernameBanned);
                return;
            }

            if (Casino.HasPlayerWithUsername(username))
            {
                writer.BaseStream.WriteByte((byte) ServerLoginResponse.AlreadyLoggedIn);
                return;
            }

            if (!DaoProvider.Dao.Login(username, password))
            {
                writer.BaseStream.WriteByte((byte) ServerLoginResponse.WrongPassword);
                return;
            }

            int chipCount = DaoProvider.Dao.GetChipCount(username);
            Casino.AddLobbyPlayer(new LobbyPlayer(username, chipCount, reader, writer));
            
            writer.BaseStream.WriteByte((byte) ServerLoginResponse.Success);
        }
    }
}