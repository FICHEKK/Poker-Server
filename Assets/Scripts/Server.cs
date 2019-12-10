using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Poker;

public class Server {
    public IPAddress Address { get; }
    public int Port { get; }
    public bool IsRunning { get; private set; }

    private TcpListener _listener;
    private Thread _listenerThread;
    private readonly List<PlayerThread> _players = new List<PlayerThread>();
    
    public Server(IPAddress address, int port) {
        Address = address;
        Port = port;
    }

    public void Start() {
        if (IsRunning) return;

        _listenerThread = new Thread(Listen) {IsBackground = true, Name = "Client Accepting"};
        _listenerThread.Start();
    }

    /// <summary>
    /// Listens for the upcoming client connections. For each new client connection,
    /// a new handler thread is created.
    /// </summary>
    private void Listen() {
        _listener = new TcpListener(Address, Port);
        _listener.Start();
        IsRunning = true;

        try {
            while (IsRunning) {
                TcpClient client = _listener.AcceptTcpClient();
                _players.Add(new PlayerThread(client));
            }
        }
        catch (SocketException e) {
            Trace.WriteLine("SocketException: " + e);
        }
        finally {
            _listener.Stop();
        }

        Trace.WriteLine("Server connection closed.");
    }

    public void Stop() {
        if (!IsRunning) return;
        
        StopListening();
        AbortServerWorkers();
    }

    private void StopListening() {
        IsRunning = false;
        _listener.Stop();
    }

    private void AbortServerWorkers() {
        foreach (PlayerThread player in _players) {
            player.Abort();
        }
        
        _players.Clear();
    }
}