using TMPro;
using UnityEngine;

public class StopButtonHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private TMP_InputField capacityInputField;

    public void StopServer()
    {
        portInputField.interactable = true;
        capacityInputField.interactable = true;
        Server.Stop();
    }

    public void OnApplicationQuit()
    {
        Server.Stop();
    }
}
