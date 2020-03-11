using System.IO;
using Dao;

namespace RequestProcessors
{
    public class RegisterRequestProcessor : IRequestProcessor
    {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer)
        {
            string password = reader.ReadLine();
            writer.BaseStream.WriteByte((byte) EvaluateProperResponse(username, password));
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