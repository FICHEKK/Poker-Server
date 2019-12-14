using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Poker;
using RequestProcessors;

/// <summary>
/// Encapsulates a thread that is responsible for processing
/// the requests from a single player.
/// </summary>
public class PlayerThread {
    private static readonly Dictionary<ClientRequest, IRequestProcessor> Processors;
    private readonly TcpClient _client;
    private readonly Thread _thread;
    
    /// <summary>
    /// Initializes request processors.
    /// </summary>
    static PlayerThread() {
        Processors = new Dictionary<ClientRequest, IRequestProcessor> {
            {ClientRequest.Login, new LoginRequestProcessor()},
            {ClientRequest.Register, new RegisterRequestProcessor()},
            {ClientRequest.JoinTable, new JoinTableRequestProcessor()},
            {ClientRequest.CreateTable, new CreateTableRequestProcessor()},
            {ClientRequest.TableList, new TableListRequestProcessor()},
            {ClientRequest.ClientData, new ClientDataRequestProcessor()},
            {ClientRequest.Logout, new LogoutRequestProcessor()}
        };
    }

    /// <summary>
    /// Constructs and starts a new thread that will handle
    /// single player's requests.
    /// </summary>
    /// <param name="client">The connection to the player.</param>
    public PlayerThread(TcpClient client) {
        _client = client;
        _thread = new Thread(ProcessRequests) {IsBackground = true};
        _thread.Start();
    }

    private void ProcessRequests() {
        using (_client)
        using (StreamReader reader = new StreamReader(_client.GetStream()))
        using (StreamWriter writer = new StreamWriter(_client.GetStream()) {AutoFlush = true}) {
            int flag = reader.BaseStream.ReadByte();
            
            while (flag != -1) {
                if (Processors.TryGetValue((ClientRequest) flag, out var processor)) {
                    processor.ProcessRequest(reader, writer);
                }
                else {
                    break;
                }
                
                flag = reader.BaseStream.ReadByte();
            }
        }
    }

    public void Abort() {
        _thread.Abort();
    }
}