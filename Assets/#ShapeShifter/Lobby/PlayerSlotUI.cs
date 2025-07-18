using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotUI : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text readyStatusText;

    public void SetInfo(string playerName, bool isReady)
    {
        Debug.Log($"[SetInfo] {playerName} — {(isReady ? "Готов" : "Не готов")}");
        playerNameText.text = playerName;
        readyStatusText.text = isReady ? "Готов" : "Не готов";
    }

    public void UpdateReadyStatus(bool isReady)
    {
        readyStatusText.text = isReady ? "Готов" : "Не готов";
    }
}