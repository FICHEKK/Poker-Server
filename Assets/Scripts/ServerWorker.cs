using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Dao;

public class ServerWorker {
    private TcpClient _client;
    private Thread _thread;

    public ServerWorker(TcpClient client) {
        Trace.WriteLine("New client connected!");
        
        _client = client;
        _thread = new Thread(ProcessClientRequests) {IsBackground = true};
        _thread.Start();
    }

    private void ProcessClientRequests() {
        using (_client)
        using (StreamReader reader = new StreamReader(_client.GetStream()))
        using (StreamWriter writer = new StreamWriter(_client.GetStream())) {
            int requestCode = reader.BaseStream.ReadByte();
            
            while (requestCode != -1) {
                ClientRequest request = (ClientRequest) requestCode;

                if (request == ClientRequest.Login) {
                    ProcessLogin(reader, writer);
                    break;
                }
                
                if (request == ClientRequest.Register) {
                    ProcessRegister(reader, writer);
                    break;
                }

                requestCode = reader.BaseStream.ReadByte();
            }
        }
        
        Trace.WriteLine("Client processing has finished!");
    }
    
    private void ProcessLogin(StreamReader reader, StreamWriter writer) {
        string username = reader.ReadLine();
        string password = reader.ReadLine();

        if (!DaoProvider.Dao.IsRegistered(username)) {
            writer.BaseStream.WriteByte((byte) ServerResponse.LoginFailedUsernameNotRegistered);
            return;
        }

        bool loginSucceeded = DaoProvider.Dao.Login(username, password);

        if (loginSucceeded) {
            writer.BaseStream.WriteByte((byte) ServerResponse.LoginSucceeded);
            writer.WriteLine(DaoProvider.Dao.GetChipCount(username).ToString());
            writer.WriteLine(DaoProvider.Dao.GetWinCount(username).ToString());
        }
        else {
            writer.BaseStream.WriteByte((byte) ServerResponse.LoginFailedWrongPassword);
        }
    }

    private void ProcessRegister(StreamReader reader, StreamWriter writer) {
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

    public void Abort() {
        _thread.Abort();
    }
}