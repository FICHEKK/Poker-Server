using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Implementation of a multi-threaded server. Each client connection
/// will be given its own thread that will process requests from that
/// client.
/// </summary>
public class Server {

    /// <summary>
    /// This server's port.
    /// </summary>
    public int Port { get; }
    
    /// <summary>
    /// Flag indicating if this server is currently running (accepting connections).
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Listener used to accept incoming connections.
    /// </summary>
    private TcpListener _listener;
    
    /// <summary>
    /// Collection of all the client connection threads.
    /// </summary>
    private readonly List<ClientThread> _clientThreads = new List<ClientThread>();
    
    /// <summary>
    /// Constructs a new server that will run on the given port.
    /// </summary>
    /// <param name="port">Port of the server.</param>
    public Server(int port) {
        Port = port;
    }

    /// <summary>
    /// Starts the server, allowing clients to connect. Calling this method
    /// will have no effect if the server is already running.
    /// </summary>
    public void Start() {
        if (IsRunning) return;

        new Thread(Listen) {Name = "Listener"}.Start();
    }

    /// <summary>
    /// Listens for the upcoming client connections. For each new client connection,
    /// a new handler thread is created.
    /// </summary>
    private void Listen() {
        _listener = new TcpListener(IPAddress.Any, Port);
        _listener.Start();
        IsRunning = true;

        try {
            while (IsRunning) {
                TcpClient client = _listener.AcceptTcpClient();
                _clientThreads.Add(new ClientThread(client));
            }
        }
        catch {
            Trace.WriteLine("Server connection closed.");
        }
        finally {
            _listener.Stop();
        }
    }

    /// <summary>
    /// Stops this server, closing the connection and disconnecting all of the
    /// currently connected clients. Calling this method will have no effect
    /// if the server is not running.
    /// </summary>
    public void Stop() {
        if (!IsRunning) return;
        
        StopListening();
        DisconnectPlayers();
    }

    /// <summary>
    /// Stops listening for incoming client connections.
    /// </summary>
    private void StopListening() {
        IsRunning = false;
        _listener.Stop();
    }

    /// <summary>
    /// Disconnects all of the clients from this server.
    /// </summary>
    private void DisconnectPlayers() {
        foreach (ClientThread client in _clientThreads) {
            client.Disconnect();
        }
        
        _clientThreads.Clear();
    }
}