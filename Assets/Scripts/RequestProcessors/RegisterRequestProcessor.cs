using Dao;

namespace RequestProcessors
{
    public class RegisterRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(Client client)
        {
            string password = client.Reader.ReadLine();
            client.Writer.BaseStream.WriteByte((byte) EvaluateProperResponse(client.Username, password));
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