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
public class Client
{
    /// <summary> A dictionary that maps client request flags to processors that process corresponding request. </summary>
    private static readonly Dictionary<ClientRequest, IRequestProcessor> Processors;
    
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
        Processors = new Dictionary<ClientRequest, IRequestProcessor>
        {
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
            {ClientRequest.LoginReward, new LoginRewardRequestProcessor()},
            {ClientRequest.LeaveTable, new LeaveTableRequestProcessor()},
            {ClientRequest.Disconnect, new DisconnectRequestProcessor()}
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
                    if (Processors.TryGetValue((ClientRequest) flag, out var processor))
                    {
                        processor.ProcessRequest(this);
                    }
                    else
                    {
                        break;
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

        Processors[ClientRequest.Disconnect].ProcessRequest(this);
    }
}