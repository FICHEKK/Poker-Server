using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Dao;
using Poker;

/// <summary>
/// Encapsulates a thread that is responsible for processing
/// the requests from a single player.
/// </summary>
public class PlayerThread {
    private readonly TcpClient _client;
    private readonly Thread _thread;

    /// <summary>
    /// Constructs and starts a new thread that will handle
    /// single player's requests.
    /// </summary>
    /// <param name="client">The connection to the player.</param>
    public PlayerThread(TcpClient client) {
        Trace.WriteLine("New client connected!");
        
        _client = client;
        _thread = new Thread(ProcessPlayerRequests) {IsBackground = true};
        _thread.Start();
    }

    private void ProcessPlayerRequests() {
        using (_client)
        using (StreamReader reader = new StreamReader(_client.GetStream()))
        using (StreamWriter writer = new StreamWriter(_client.GetStream())) {
            int requestCode = reader.BaseStream.ReadByte();
            
            while (requestCode != -1) {
                ClientRequest request = (ClientRequest) requestCode;
                Trace.WriteLine("Received code: " + request);

                if (request == ClientRequest.Login) {
                    Trace.WriteLine("Login!");
                    ProcessLogin(reader, writer);
                } 
                else if (request == ClientRequest.TableList) {
                    Trace.WriteLine("Tables!");
                    SendTableList(writer);
                }
                else if (request == ClientRequest.Register) {
                    ProcessRegister(reader, writer);
                    break;
                }
                else {
                    break;
                }

                requestCode = reader.BaseStream.ReadByte();
            }
        }
        
        Trace.WriteLine("Client processing has finished!");
    }

    private void SendTableList(StreamWriter writer) {
        Trace.WriteLine("Writing tables..");
        writer.WriteLine(Casino.TableCount);
        
        foreach (string tableName in Casino.TableNames) {
            Table table = Casino.GetTable(tableName);
            
            writer.WriteLine(tableName);
            writer.WriteLine(table.SmallBlind);
            writer.WriteLine(table.PlayerCount);
            writer.WriteLine(table.MaxPlayers);
        }
        
        writer.Flush();
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
            writer.Flush();
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