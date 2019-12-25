using System.IO;
using Dao;

namespace RequestProcessors {
    public class RegisterRequestProcessor : IRequestProcessor {
        public void ProcessRequest(string username, StreamReader reader, StreamWriter writer) {
            string password = reader.ReadLine();

            if (DaoProvider.Dao.IsRegistered(username)) {
                writer.BaseStream.WriteByte((byte) ServerRegistrationResponse.UsernameTaken);
                return;
            }

            if (!DaoProvider.Dao.Register(username, password)) {
                writer.BaseStream.WriteByte((byte) ServerRegistrationResponse.DatabaseError);
                return;
            }
            
            writer.BaseStream.WriteByte((byte) ServerRegistrationResponse.Success);
        }
    }
}