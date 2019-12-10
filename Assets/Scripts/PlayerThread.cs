using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using RequestProcessors;

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

    private static Dictionary<ClientRequest, IRequestProcessor> _processors;
    static PlayerThread() {
        _processors = new Dictionary<ClientRequest, IRequestProcessor> {
            {ClientRequest.Login, new LoginRequestProcessor()},
            {ClientRequest.Register, new RegisterRequestProcessor()},
            {ClientRequest.JoinTable, new JoinTableRequestProcessor()},
            {ClientRequest.CreateTable, new CreateTableRequestProcessor()},
            {ClientRequest.TableList, new TableListRequestProcessor()}
        };
    }

    private void ProcessPlayerRequests() {
        using (_client)
        using (StreamReader reader = new StreamReader(_client.GetStream()))
        using (StreamWriter writer = new StreamWriter(_client.GetStream())) {
            int flag = reader.BaseStream.ReadByte();
            
            while (flag != -1) {
                if (_processors.TryGetValue((ClientRequest) flag, out var processor)) {
                    processor.ProcessRequest(reader, writer);
                }
                else {
                    break;
                }

                flag = reader.BaseStream.ReadByte();
            }
        }
        
        Trace.WriteLine("Client processing has finished!");
    }

    public void Abort() {
        _thread.Abort();
    }
}