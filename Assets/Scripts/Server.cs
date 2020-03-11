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
public static class Server
{
    /// <summary> Raised each time the server is started. </summary>
    public static event EventHandler ServerStarted;

    /// <summary> Raised each time the server is stopped. </summary>
    public static event EventHandler ServerStopped;

    /// <summary> Flag indicating if this server is currently running (accepting connections). </summary>
    public static bool IsRunning { get; private set; }

    /// <summary> The number of clients currently connected to this server. </summary>
    public static int ClientCount => Clients.Count;

    /// <summary> The maximum number of simultaneous client connections. </summary>
    public static int Capacity { get; private set; }

    /// <summary> Listener used to accept incoming connections. </summary>
    private static TcpListener _listener;

    /// <summary> Collection of all the client connections. </summary>
    private static readonly List<Client> Clients = new List<Client>();

    /// <summary> Starts the server on the specified port, allowing clients to connect. </summary>
    public static void Start(int port, int capacity)
    {
        if (IsRunning) return;

        Capacity = capacity;
        new Thread(() => Listen(port)) {Name = "Listener"}.Start();

        ServerStarted?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Listens for the upcoming client connections.
    /// For each new client connection, a new handler thread is created.
    /// </summary>
    private static void Listen(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        IsRunning = true;

        try
        {
            while (IsRunning)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Clients.Add(new Client(client));
            }
        }
        catch
        {
            Trace.WriteLine("Server connection closed.");
        }
        finally
        {
            _listener.Stop();
        }

        ServerStopped?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Stops this server, closing the connection and disconnecting all of the currently connected
    /// clients. Calling this method will have no effect if the server is not running.
    /// </summary>
    public static void Stop()
    {
        if (!IsRunning) return;

        StopListening();
        DisconnectPlayers();
    }

    public static void DisconnectClient(string username)
    {
        for (var index = 0; index < Clients.Count; index++)
        {
            Client client = Clients[index];

            if (client.Username == username)
            {
                client.Connection.Close();
                Clients.RemoveAt(index);
                break;
            }
        }
    }

    /// <summary> Stops listening for incoming client connections. </summary>
    private static void StopListening()
    {
        IsRunning = false;
        _listener.Stop();
    }

    /// <summary> Disconnects all of the clients from this server. </summary>
    private static void DisconnectPlayers()
    {
        foreach (Client client in Clients)
        {
            client.Connection.Close();
        }

        Clients.Clear();
    }
}