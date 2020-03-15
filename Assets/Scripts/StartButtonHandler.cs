using TMPro;
using UnityEngine;

public class StartButtonHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private TMP_InputField capacityInputField;

    private void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        portInputField.interactable = false;
        capacityInputField.interactable = false;
        Server.Start(int.Parse(portInputField.text), int.Parse(capacityInputField.text));
    }
}
