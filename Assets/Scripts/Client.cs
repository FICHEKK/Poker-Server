using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Logger;
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

    private StreamWriter _writer;
    private StreamReader _reader;

    private readonly List<string> _requestLogBuffer = new List<string>();
    
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
            using (_writer = new StreamWriter(Connection.GetStream()) {AutoFlush = true})
            using (_reader = new StreamReader(Connection.GetStream()))
            {
                int flag = _reader.BaseStream.ReadByte();
                Username = _reader.ReadLine();

                while (flag != -1)
                {
                    if (RequestToProcessorFactory.TryGetValue((ClientRequest) flag, out var processorFactory))
                    {
                        var processor = processorFactory();
                        processor.ReadPayloadData(this);
                        
                        LogRequest((ClientRequest) flag);

                        if (processor.CanWait)
                        {
                            Casino.GetTablePlayer(Username).TableController.Enqueue(processor.ProcessRequest);
                        }
                        else
                        {
                            processor.ProcessRequest();
                        }
                    }

                    flag = _reader.BaseStream.ReadByte();
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
    
    public string ReadLine()
    {
        var line = _reader.ReadLine();
        _requestLogBuffer.Add(line);
        return line;
    }

    private void LogRequest(ClientRequest request)
    {
        var sb = new StringBuilder();
        sb.Append($"[{DateTime.Now:yyyy.MM.dd. HH:mm:ss} REQ {Username}] {request} ");
        
        foreach (var logPiece in _requestLogBuffer)
            sb.Append(logPiece).Append(' ');
        
        _requestLogBuffer.Clear();
        LoggerProvider.Logger.Log(sb.Append(Environment.NewLine).ToString());
    }

    public class Package
    {
        private readonly Client[] _recipients;
        private readonly List<object> _data = new List<object>();

        /// <summary>Constructs a new package that will be sent to a single client.</summary>
        public Package(Client recipient) : this(new[] {recipient}) { }
        
        /// <summary>Constructs a new package that will be sent to multiple clients.</summary>
        public Package(Client[] recipients)
        {
            _recipients = recipients ?? throw new ArgumentNullException(nameof(recipients));

            if (_recipients.Any(recipient => recipient == null))
                throw new ArgumentException("A recipient must be non-null.");
        }

        /// <summary> Appends additional data to this package and returns this package. </summary>
        public Package Append(object data)
        {
            _data.Add(data);
            return this;
        }
        
        /// <summary>Maps and appends every item from the provided collection to this package and returns this package.</summary>
        public Package Append<T, TResult>(IEnumerable<T> items, Func<T, TResult> mapper)
        {
            foreach (var item in items) _data.Add(mapper(item));
            return this;
        }

        /// <summary> Sends all of the previously appended data to the recipients. </summary>
        public void Send()
        {
            if (_recipients.Length == 0) return;
            
            foreach (var recipient in _recipients)
            {
                foreach (var value in _data)
                {
                    var response = value as ServerResponse?;

                    if (response != null)
                    {
                        recipient._writer.BaseStream.WriteByte((byte) response.Value);
                    }
                    else
                    {
                        recipient._writer.WriteLine(value.ToString());
                    }
                }
            }

            LogResponse();
        }

        private void LogResponse()
        {
            var sb = new StringBuilder();
            sb.Append($"[{DateTime.Now:yyyy.MM.dd. HH:mm:ss} RES {string.Join(",", _recipients.Select(r => r.Username).ToArray())}] ");
            
            foreach (var value in _data) 
                sb.Append(value).Append(' ');
            
            LoggerProvider.Logger.Log(sb.Append(Environment.NewLine).ToString());
        }
    }
}