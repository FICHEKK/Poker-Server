using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using RequestProcessors;

/// <summary>
/// Encapsulates a thread that is responsible for processing
/// the requests from a single client.
/// </summary>
public class ClientThread {
    
    /// <summary> A dictionary that maps client request flags to processors that process corresponding request. </summary>
    private static readonly Dictionary<ClientRequest, IRequestProcessor> Processors;
    
    /// <summary> Username of the client. </summary>
    private string _username;

    /// <summary> The connection to the client. </summary>
    private readonly TcpClient _client;

    /// <summary> Initializes request processors. </summary>
    static ClientThread() {
        Processors = new Dictionary<ClientRequest, IRequestProcessor> {
            {ClientRequest.Login, new LoginRequestProcessor()},
            {ClientRequest.Register, new RegisterRequestProcessor()},
            {ClientRequest.JoinTable, new JoinTableRequestProcessor()},
            {ClientRequest.CreateTable, new CreateTableRequestProcessor()},
            {ClientRequest.TableList, new TableListRequestProcessor()},
            {ClientRequest.ClientData, new ClientDataRequestProcessor()},
            {ClientRequest.Logout, new LogoutRequestProcessor()},
            {ClientRequest.Check, new CheckRequestProcessor()},
            {ClientRequest.Call, new CallRequestProcessor()},
            {ClientRequest.Fold, new FoldRequestProcessor()},
            {ClientRequest.Raise, new RaiseRequestProcessor()},
            {ClientRequest.AllIn, new AllInRequestProcessor()},
            {ClientRequest.Disconnect, new DisconnectRequestProcessor()},
            {ClientRequest.LoginReward, new LoginRewardRequestProcessor()},
            {ClientRequest.LeaveTable, new LeaveTableRequestProcessor()}
        };
    }

    /// <summary> Constructs and starts a new thread that will handle single client's requests. </summary>
    /// <param name="client"> Connection to the client. </param>
    public ClientThread(TcpClient client) {
        _client = client;
        new Thread(ProcessRequests).Start();
    }

    /// <summary> Processes incoming client requests. </summary>
    private void ProcessRequests() {
        try {
            using (_client)
            using (StreamReader reader = new StreamReader(_client.GetStream()))
            using (StreamWriter writer = new StreamWriter(_client.GetStream()) {AutoFlush = true}) {
                int flag = reader.BaseStream.ReadByte();
                _username = reader.ReadLine();

                while (flag != -1) {
                    if (Processors.TryGetValue((ClientRequest) flag, out var processor)) {
                        processor.ProcessRequest(_username, reader, writer);
                    }
                    else {
                        break;
                    }

                    flag = reader.BaseStream.ReadByte();
                }
            }
        }
        catch (Exception e) {
            Trace.WriteLine(e);
            Trace.WriteLine("Disconnecting the client...");
        }
        
        Processors[ClientRequest.Disconnect].ProcessRequest(_username, null, null);
    }

    /// <summary> Disconnects the client from the server. </summary>
    public void Disconnect() {
        _client.Close();
    }
}