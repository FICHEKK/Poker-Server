using System.IO;
using Dao;

namespace RequestProcessors {
    public class RegisterRequestProcessor : IRequestProcessor {
        public void ProcessRequest(StreamReader reader, StreamWriter writer) {
            string username = reader.ReadLine();
            string password = reader.ReadLine();

            if (DaoProvider.Dao.IsRegistered(username)) {
                writer.BaseStream.WriteByte((byte) ServerResponse.RegistrationFailedUsernameAlreadyTaken);
                return;
            }

            bool wasNoError = DaoProvider.Dao.Register(username, password);

            if (wasNoError) {
                writer.BaseStream.WriteByte((byte) ServerResponse.RegistrationSucceeded);
            }
            else {
                writer.BaseStream.WriteByte((byte) ServerResponse.RegistrationFailedIOError);
            }
        }
    }
}