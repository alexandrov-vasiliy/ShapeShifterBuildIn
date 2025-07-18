using Mirror;
using UnityEngine;
using System;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    public static event Action<CustomRoomPlayer> OnRoomPlayerJoined;
    public static event Action<CustomRoomPlayer> OnRoomPlayerLeft;
    public static event Action<CustomRoomPlayer, bool> OnRoomPlayerStateChanged;

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnRoomPlayerJoined?.Invoke(this);
    }

    public override void OnClientExitRoom()
    {
        base.OnClientExitRoom();
        OnRoomPlayerLeft?.Invoke(this);
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        OnRoomPlayerStateChanged?.Invoke(this, newReadyState);
    }
}