using TMPro;
using UnityEngine;

public class StopButtonHandler : MonoBehaviour {
    [SerializeField] private TMP_InputField portInputField;
    
    public void StopServer() {
        portInputField.interactable = true;
        Server.Stop();
    }
    
    public void OnApplicationQuit() {
        Server.Stop();
    }
}
