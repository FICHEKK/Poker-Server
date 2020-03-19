using Dao;
using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class LoginRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            string password = client.Reader.ReadLine();

            ServerLoginResponse response = EvaluateProperResponse(client.Username, password);

            if (response == ServerLoginResponse.Success)
            {
                int chipCount = DaoProvider.Dao.GetChipCount(client.Username);
                Casino.AddLobbyPlayer(new LobbyPlayer(client.Username, chipCount, client.Reader, client.Writer));
                client.IsLoggedIn = true;
            }
            
            client.Writer.BaseStream.WriteByte((byte) response);
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