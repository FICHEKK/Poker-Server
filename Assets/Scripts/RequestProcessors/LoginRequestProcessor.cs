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
            _password = _client.ReadLine();
        }

        public void ProcessRequest()
        {
            var response = EvaluateProperResponse(_client.Username, _password);

            if (response == ServerResponse.LoginSuccess)
            {
                var chipCount = DaoProvider.Dao.GetChipCount(_client.Username);
                Casino.AddLobbyPlayer(new LobbyPlayer(_client, chipCount));
                _client.IsLoggedIn = true;
            }

            new Client.Package(_client).Append(response).Send();
        }

        private static ServerResponse EvaluateProperResponse(string username, string password)
        {
            if (Server.ClientCount > Server.Capacity)
                return ServerResponse.LoginServerFull;
            
            if (!DaoProvider.Dao.IsRegistered(username))
                return ServerResponse.LoginUsernameNotRegistered;
            
            if (DaoProvider.Dao.IsBanned(username))
                return ServerResponse.LoginUsernameBanned;
            
            if (Casino.HasPlayerWithUsername(username))
                return ServerResponse.LoginAlreadyLoggedIn;
            
            if (!DaoProvider.Dao.Login(username, password))
                return ServerResponse.LoginWrongPassword;
            
            return ServerResponse.LoginSuccess;
        }
    }
}