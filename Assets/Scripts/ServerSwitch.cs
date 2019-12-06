using System;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerSwitch : MonoBehaviour {
    [SerializeField] private TMP_InputField addressInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Text buttonText;
    [SerializeField] private Text consoleText;
    
    private static readonly Color StartButtonGreen = new Color(0f, 0.5f, 0f);
    private static readonly Color StopButtonRed = Color.red;
    
    private Server _server;
    private bool _isRunning;

    public void SwitchServerStatus() {
        if (_isRunning) {
            StopServer();
        }
        else {
            StartServer();
        }
    }
    
    private void StartServer() {
        IPAddress address = IPAddress.Parse(addressInputField.text);
        int port = int.Parse(portInputField.text);

        _server = new Server(address, port);
        _server.Start();
        _isRunning = true;
        
        buttonText.text = "Stop";
        buttonImage.color = StopButtonRed;
        consoleText.text += "Server started on " + address + ":" + port + "." + Environment.NewLine;
    }

    private void StopServer() {
        _server.Stop();
        _isRunning = false;

        buttonText.text = "Start";
        buttonImage.color = StartButtonGreen;
        consoleText.text += "Server stopped." + Environment.NewLine;
    }
    
    public void OnApplicationQuit() {
        if (_isRunning) {
            _server.Stop();
        }
    }
}