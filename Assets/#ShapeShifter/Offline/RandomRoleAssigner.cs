using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

/// <summary>
/// Поместите в сцену (на тот же GO, что и NetworkRoomManager, или рядом).
/// После того как все отметились Ready, случайно назначает одного охотника.
/// </summary>
public class RandomRoleAssigner : NetworkBehaviour
{
    // Запускаемся только на сервере
    public override void OnStartServer()
    {
        StartCoroutine(AssignRolesWhenEveryoneReady());
    }

    IEnumerator AssignRolesWhenEveryoneReady()
    {
        // ждём, пока allPlayersReady у менеджера станет true
        var room = FindObjectOfType<NetworkRoomManager>();
        while (room == null || !room.allPlayersReady)   // поле менеджера allPlayersReady :contentReference[oaicite:0]{index=0}
            yield return null;

        yield return null; // ещё кадр — чтобы все Player‑объекты успели заспауниться

        var players = FindObjectsOfType<_ShapeShifter.Player.Scripts.ChangeRole>().ToList();
        if (players.Count == 0) yield break;

        int hunterIndex = Random.Range(0, players.Count); // случайный номер

        for (int i = 0; i < players.Count; i++)
        {
            var role = (i == hunterIndex)
                ? _ShapeShifter.Player.Scripts.Role.Hunter
                : _ShapeShifter.Player.Scripts.Role.Survivor;

            players[i].SetRole(role);                   // сервер меняет SyncVar
        }
    }
}


