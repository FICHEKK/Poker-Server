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

                if (request == ClientRequest.Login) {
                    ProcessLogin(reader, writer);
                } 
                else if (request == ClientRequest.TableList) {
                    ProcessTableList(writer);
                }
                else if (request == ClientRequest.Register) {
                    ProcessRegister(reader, writer);
                    break;
                }
                else if (request == ClientRequest.CreateTable) {
                    ProcessCreateTable(reader, writer);
                }
                else {
                    break;
                }

                requestCode = reader.BaseStream.ReadByte();
            }
        }
        
        Trace.WriteLine("Client processing has finished!");
    }

    private void ProcessCreateTable(StreamReader reader, StreamWriter writer) {
        string tableTitle = reader.ReadLine();
        int smallBlind = int.Parse(reader.ReadLine());
        int maxPlayers = int.Parse(reader.ReadLine());

        if (Casino.HasTableWithTitle(tableTitle)) {
            writer.BaseStream.WriteByte((byte) ServerResponse.TableCreationFailedTitleAlreadyTaken);
        }
        else {
            Casino.AddTable(tableTitle, new Table(smallBlind, maxPlayers));
            writer.BaseStream.WriteByte((byte) ServerResponse.TableCreationSucceeded);
        }
    }

    private void ProcessTableList(StreamWriter writer) {
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