using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Implementation of a multi-threaded server. Each client connection will be
/// given its own thread that will process requests from that client.
/// </summary>
public static class Server {

    /// <summary> Raised each time the server is started. </summary>
    public static event EventHandler ServerStarted;
    
    /// <summary> Raised each time the server is stopped. </summary>
    public static event EventHandler ServerStopped;

    /// <summary> Flag indicating if this server is currently running (accepting connections). </summary>
    public static bool IsRunning { get; private set; }

    /// <summary> Listener used to accept incoming connections. </summary>
    private static TcpListener _listener;

    /// <summary> Collection of all the client connection threads. </summary>
    private static readonly List<ClientThread> ClientThreads = new List<ClientThread>();

    /// <summary> Starts the server on the specified port, allowing clients to connect. </summary>
    public static void Start(int port) {
        if (IsRunning) return;
        
        new Thread(() => Listen(port)) {Name = "Listener"}.Start();
        
        ServerStarted?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Listens for the upcoming client connections.
    /// For each new client connection, a new handler thread is created.
    /// </summary>
    private static void Listen(int port) {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        IsRunning = true;

        try {
            while (IsRunning) {
                TcpClient client = _listener.AcceptTcpClient();
                ClientThreads.Add(new ClientThread(client));
            }
        }
        catch {
            Trace.WriteLine("Server connection closed.");
        }
        finally {
            _listener.Stop();
        }
        
        ServerStopped?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Stops this server, closing the connection and disconnecting all of the currently connected
    /// clients. Calling this method will have no effect if the server is not running.
    /// </summary>
    public static void Stop() {
        if (!IsRunning) return;
        
        StopListening();
        DisconnectPlayers();
    }

    /// <summary> Stops listening for incoming client connections. </summary>
    private static void StopListening() {
        IsRunning = false;
        _listener.Stop();
    }

    /// <summary> Disconnects all of the clients from this server. </summary>
    private static void DisconnectPlayers() {
        foreach (ClientThread client in ClientThreads) {
            client.Disconnect();
        }
        
        ClientThreads.Clear();
    }
}