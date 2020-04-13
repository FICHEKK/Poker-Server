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
            _password = client.Reader.ReadLine();
        }

        public void ProcessRequest()
        {
            _client.Writer.BaseStream.WriteByte((byte) EvaluateProperResponse(_client.Username, _password));
        }

        private static ServerRegistrationResponse EvaluateProperResponse(string username, string password)
        {
            if (DaoProvider.Dao.IsRegistered(username))
                return ServerRegistrationResponse.UsernameTaken;
            
            if (!DaoProvider.Dao.Register(username, password))
                return ServerRegistrationResponse.DatabaseError;
            
            return ServerRegistrationResponse.Success;
        }
    }
}