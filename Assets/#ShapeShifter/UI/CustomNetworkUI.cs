using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomNetworkUI : MonoBehaviour
{
    public TMP_InputField addressInput;
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;

    void Start()
    {
        if (addressInput != null)
            addressInput.text = "localhost"; // или 127.0.0.1
        
        hostButton.onClick.AddListener(() => {
            NetworkManager.singleton.StartHost();
        });

        clientButton.onClick.AddListener(() => {
            NetworkManager.singleton.networkAddress = addressInput.text;
            NetworkManager.singleton.StartClient();
        });

        serverButton.onClick.AddListener(() => {
            NetworkManager.singleton.StartServer();
        });
    }
}