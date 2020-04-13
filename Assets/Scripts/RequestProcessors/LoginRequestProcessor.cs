using Dao;
using Poker;
using Poker.Players;

namespace RequestProcessors
{
    public class LoginRequestProcessor : IClientRequestProcessor
    {
        public bool CanWait => false;
        private Client _client;
        private string _password;

        public void ReadPayloadData(Client client)
        {
            _client = client;
            _password = client.Reader.ReadLine();
        }

        public void ProcessRequest()
        {
            ServerLoginResponse response = EvaluateProperResponse(_client.Username, _password);

            if (response == ServerLoginResponse.Success)
            {
                int chipCount = DaoProvider.Dao.GetChipCount(_client.Username);
                Casino.AddLobbyPlayer(new LobbyPlayer(_client.Username, chipCount, _client.Reader, _client.Writer));
                _client.IsLoggedIn = true;
            }
            
            _client.Writer.BaseStream.WriteByte((byte) response);
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