using TMPro;
using UnityEngine;

public class StartButtonHandler : MonoBehaviour {
    [SerializeField] private TMP_InputField portInputField;

    public void StartServer() {
        portInputField.interactable = false;
        Server.Start(int.Parse(portInputField.text));
    }
}
