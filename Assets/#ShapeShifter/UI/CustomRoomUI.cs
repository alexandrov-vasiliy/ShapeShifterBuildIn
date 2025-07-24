using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CustomRoomUI : MonoBehaviour
{
    public Button readyButton;
    //public Button leaveButton;
    public TMP_Text statusText;

    private NetworkRoomPlayer roomPlayer;

    void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    IEnumerator WaitForPlayer()
    {
        // Ждём, пока identity появится
        while (NetworkClient.connection == null || NetworkClient.connection.identity == null)
            yield return null;

        roomPlayer = NetworkClient.connection.identity.GetComponent<NetworkRoomPlayer>();

        if (roomPlayer != null)
        {
            readyButton.onClick.AddListener(() => {
                roomPlayer.CmdChangeReadyState(!roomPlayer.readyToBegin);
            });
/*
            leaveButton.onClick.AddListener(() => {
                if (NetworkServer.active)
                    NetworkManager.singleton.StopHost();
                else
                    NetworkManager.singleton.StopClient();
            });*/
        }
    }

    void Update()
    {
        if (roomPlayer != null)
        {
            statusText.text = roomPlayer.readyToBegin ? "Готов" : "Не готов";
        }
    }
}