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

            ServerLoginResponse response = EvaluateProperResponse(username, password);

            if (response == ServerLoginResponse.Success)
            {
                int chipCount = DaoProvider.Dao.GetChipCount(username);
                Casino.AddLobbyPlayer(new LobbyPlayer(username, chipCount, reader, writer));
            }
            
            writer.BaseStream.WriteByte((byte) response);
        }

        private static ServerLoginResponse EvaluateProperResponse(string username, string password)
        {
            if (Server.ClientCount > Server.Capacity)
                return ServerLoginResponse.ServerFull;
            
            if (!DaoProvider.Dao.IsRegistered(username))
                return ServerLoginResponse.UsernameNotRegistered;
            
            if (DaoProvider.Dao.IsBanned(username))
                return ServerLoginResponse.UsernameBanned;
            
            if (Casino.HasPlayerWithUsername(username))
                return ServerLoginResponse.AlreadyLoggedIn;
            
            if (!DaoProvider.Dao.Login(username, password))
                return ServerLoginResponse.WrongPassword;
            
            return ServerLoginResponse.Success;
        }
    }
}