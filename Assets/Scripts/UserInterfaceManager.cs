using System;
using Poker;
using Poker.EventArguments.Casino;
using TMPro;
using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyPlayerCountText;
    [SerializeField] private TMP_Text tablePlayerCountText;
    [SerializeField] private TMP_Text tableCountText;

    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject stopButton;

    [SerializeField] private TMP_Text consoleText;

    private int _lobbyPlayerCount;
    private int _tablePlayerCount;
    private int _tableCount;

    private void Awake()
    {
        Server.ServerStarted += ServerStartedEventHandler;
        Server.ServerStopped += ServerStoppedEventHandler;

        Casino.LobbyPlayerAdded += LobbyPlayerAddedEventHandler;
        Casino.LobbyPlayerRemoved += LobbyPlayerRemovedEventHandler;

        Casino.TablePlayerAdded += TablePlayerAddedEventHandler;
        Casino.TablePlayerRemoved += TablePlayerRemovedEventHandler;

        Casino.TableAdded += TableAddedEventHandler;
        Casino.TableRemoved += TableRemovedEventHandler;
    }

    private void ServerStartedEventHandler(object sender, EventArgs e)
    {
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            startButton.SetActive(false);
            stopButton.SetActive(true);
            AppendToConsole("Server started.");
        });
    }

    private void ServerStoppedEventHandler(object sender, EventArgs e)
    {
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            startButton.SetActive(true);
            stopButton.SetActive(false);
            AppendToConsole("Server stopped.");
        });
    }

    private void LobbyPlayerAddedEventHandler(object sender, LobbyPlayerAddedEventArgs e)
    {
        _lobbyPlayerCount++;
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            lobbyPlayerCountText.text = "In-Lobby: " + _lobbyPlayerCount;
            AppendToConsole("Player '" + e.Username + "' joined the lobby.");
        });
    }

    private void LobbyPlayerRemovedEventHandler(object sender, LobbyPlayerRemovedEventArgs e)
    {
        _lobbyPlayerCount--;
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            lobbyPlayerCountText.text = "In-Lobby: " + _lobbyPlayerCount;
            AppendToConsole("Player '" + e.Username + "' left the lobby.");
        });
    }

    private void TablePlayerAddedEventHandler(object sender, TablePlayerAddedEventArgs e)
    {
        _tablePlayerCount++;
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            tablePlayerCountText.text = "On-Table: " + _tablePlayerCount;
            AppendToConsole("Player '" + e.Username + "' joined table '" + e.TableController.Title + "'.");
        });
    }

    private void TablePlayerRemovedEventHandler(object sender, TablePlayerRemovedEventArgs e)
    {
        _tablePlayerCount--;
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            tablePlayerCountText.text = "On-Table: " + _tablePlayerCount;
            AppendToConsole("Player '" + e.Username + "' left table '" + e.TableController.Title + "'.");
        });
    }

    private void TableAddedEventHandler(object sender, TableAddedEventArgs e)
    {
        _tableCount++;
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            tableCountText.text = "Tables: " + _tableCount;
            AppendToConsole("Table '" + e.TableController.Title + "' has been added to the casino.");
        });
    }

    private void TableRemovedEventHandler(object sender, TableRemovedEventArgs e)
    {
        _tableCount--;
        MainThreadExecutor.Instance.Enqueue(() =>
        {
            tableCountText.text = "Tables: " + _tableCount;
            AppendToConsole("Table '" + e.Title + "' has been removed from the casino.");
        });
    }

    private void AppendToConsole(string text)
    {
        consoleText.text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text + Environment.NewLine;
    }
}
