using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RoomContainerListManager : MonoBehaviour
{
    public Transform container;              // Сюда будут добавляться слоты
    public GameObject playerSlotPrefab;      // Префаб слота (PlayerSlotUI)
    
    private Dictionary<CustomRoomPlayer, PlayerSlotUI> uiSlots = new();

    private void OnEnable()
    {
        
        CustomRoomPlayer.OnRoomPlayerStateChanged += OnPlayerStateChanged;
        CustomRoomPlayer.OnRoomPlayerJoined += OnPlayerJoined;
        CustomRoomPlayer.OnRoomPlayerLeft += OnPlayerLeft;
    }

    private void OnDisable()
    {
        CustomRoomPlayer.OnRoomPlayerStateChanged -= OnPlayerStateChanged;
        CustomRoomPlayer.OnRoomPlayerJoined -= OnPlayerJoined;
        CustomRoomPlayer.OnRoomPlayerLeft -= OnPlayerLeft;
    }

    void OnPlayerJoined(CustomRoomPlayer player)
    {
        GameObject go = Instantiate(playerSlotPrefab, container);
        var ui = go.GetComponent<PlayerSlotUI>();
        ui.SetInfo($"Игрок {player.index + 1}", player.readyToBegin);
        uiSlots[player] = ui;
    }

    void OnPlayerLeft(CustomRoomPlayer player)
    {
        if (uiSlots.TryGetValue(player, out var ui))
        {
            Destroy(ui.gameObject);
            uiSlots.Remove(player);
        }
    }

    void OnPlayerStateChanged(CustomRoomPlayer player, bool isReady)
    {
        if (uiSlots.TryGetValue(player, out var ui))
        {
            ui.UpdateReadyStatus(isReady);
        }
    }
}