using Mirror;
using UnityEngine;
using System;


public enum Role : byte
{
    Hunter,
    Survivor
}


public class CustomRoomPlayer : NetworkRoomPlayer
{
    public static event Action<CustomRoomPlayer> OnRoomPlayerJoined;
    public static event Action<CustomRoomPlayer> OnRoomPlayerLeft;
    public static event Action<CustomRoomPlayer, bool> OnRoomPlayerStateChanged;


    [SyncVar] public Role assignedRole = Role.Survivor;


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