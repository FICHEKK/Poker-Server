using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Poker;
using RequestProcessors;

/// <summary>
/// Encapsulates a thread that is responsible for processing
/// the requests from a single client.
/// </summary>
public class Client
{
    /// <summary> Maps client request to factory method for creating the appropriate request processor. </summary>
    private static readonly Dictionary<ClientRequest, Func<IClientRequestProcessor>> RequestToProcessorFactory;

    /// <summary> This client's username. </summary>
    public string Username { get; private set; }
    
    /// <summary> Flag indicating whether this client is logged into the system (passed the log-in phase). </summary>
    public bool IsLoggedIn { get; set; }

    /// <summary> The connection to the remote client. </summary>
    public TcpClient Connection { get; }
    
    public StreamWriter Writer { get; private set; }

    public StreamReader Reader { get; private set; }
    
    /// <summary> This client's unique connection identifier. </summary>
    public Guid Identifier { get; }

    /// <summary> Initializes request processors. </summary>
    static Client()
    {
        RequestToProcessorFactory = new Dictionary<ClientRequest, Func<IClientRequestProcessor>>
        {
            {ClientRequest.Login, () => new LoginRequestProcessor()},
            {ClientRequest.Register, () => new RegisterRequestProcessor()},
            {ClientRequest.JoinTable, () => new JoinTableRequestProcessor()},
            {ClientRequest.CreateTable, () => new CreateTableRequestProcessor()},
            {ClientRequest.TableList, () => new TableListRequestProcessor()},
            {ClientRequest.ClientData, () => new ClientDataRequestProcessor()},
            {ClientRequest.Logout, () => new LogoutRequestProcessor()},
            {ClientRequest.Check, () => new CheckRequestProcessor()},
            {ClientRequest.Call, () => new CallRequestProcessor()},
            {ClientRequest.Fold, () => new FoldRequestProcessor()},
            {ClientRequest.Raise, () => new RaiseRequestProcessor()},
            {ClientRequest.AllIn, () => new AllInRequestProcessor()},
            {ClientRequest.LoginReward, () => new LoginRewardRequestProcessor()},
            {ClientRequest.LeaveTable, () => new LeaveTableRequestProcessor()},
            {ClientRequest.Disconnect, () => new DisconnectRequestProcessor()},
            {ClientRequest.SendChatMessage, () => new SendChatMessageRequestProcessor()},
        };
    }

    /// <summary> Constructs and starts a new thread that will handle single client's requests. </summary>
    /// <param name="connection"> Connection to the client. </param>
    /// <param name="identifier"> Unique connection identifier. </param>
    public Client(TcpClient connection, Guid identifier)
    {
        Connection = connection;
        Identifier = identifier;
        new Thread(ProcessRequests).Start();
    }

    /// <summary> Processes incoming client requests. </summary>
    private void ProcessRequests()
    {
        try
        {
            using (Connection)
            using (Writer = new StreamWriter(Connection.GetStream()) {AutoFlush = true})
            using (Reader = new StreamReader(Connection.GetStream()))
            {
                int flag = Reader.BaseStream.ReadByte();
                Username = Reader.ReadLine();

                while (flag != -1)
                {
                    if (RequestToProcessorFactory.TryGetValue((ClientRequest) flag, out var processorFactory))
                    {
                        var processor = processorFactory();
                        processor.ReadPayloadData(this);

                        if (processor.CanWait)
                        {
                            Casino.GetTablePlayer(Username).Table.RequestProcessors.Add(processor);
                        }
                        else
                        {
                            processor.ProcessRequest();
                        }
                    }

                    flag = Reader.BaseStream.ReadByte();
                }
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            Trace.WriteLine("Disconnecting the client...");
        }

        // Disconnect the client
        var disconnectProcessor = RequestToProcessorFactory[ClientRequest.Disconnect]();
        disconnectProcessor.ReadPayloadData(this);
        disconnectProcessor.ProcessRequest();
    }
}