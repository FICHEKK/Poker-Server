using Dao;

namespace RequestProcessors
{
    public class RegisterRequestProcessor : IClientRequestProcessor
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
            new Client.Package(_client)
                .Append(EvaluateProperResponse(_client.Username, _password))
                .Send();
        }

        private static ServerResponse EvaluateProperResponse(string username, string password)
        {
            if (DaoProvider.Dao.IsRegistered(username))
                return ServerResponse.RegistrationUsernameTaken;
            
            if (!DaoProvider.Dao.Register(username, password))
                return ServerResponse.RegistrationDatabaseError;
            
            return ServerResponse.RegistrationSuccess;
        }
    }
}