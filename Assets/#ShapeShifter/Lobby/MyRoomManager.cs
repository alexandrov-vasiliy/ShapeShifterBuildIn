using _ShapeShifter.Player.Scripts;
using Mirror;
using UnityEngine;


public class MyRoomManager : NetworkRoomManager
{
    [Header("Prefabs")]
    public GameObject hunterPrefab;
    public GameObject survivorPrefab;
    
    bool rolesAssigned = false;

    public override void OnRoomServerPlayersReady()
    {
        
        Debug.Log(roomSlots.Count);
        if (rolesAssigned || roomSlots.Count == 0) return;
        Debug.Log(rolesAssigned);
        int hunterIndex = UnityEngine.Random.Range(0, roomSlots.Count);

        int current = 0;
        foreach (var slot in roomSlots)              // HashSet не индексируется
        {
            var rp = slot as CustomRoomPlayer;       // ваш наследник NetworkRoomPlayer
            if (rp == null) continue;

            rp.assignedRole = (current == hunterIndex)
                ? Role.Hunter
                : Role.Survivor;

            current++;                               // счётчик «ручной»
        }

        rolesAssigned = true;
        ServerChangeScene(GameplayScene);
    }


    public override GameObject OnRoomServerCreateGamePlayer(
        NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        // 1. Какой префаб нужен?
        var rp = roomPlayer.GetComponent<CustomRoomPlayer>();
        GameObject prefab = (rp.assignedRole == Role.Hunter)
            ? hunterPrefab
            : survivorPrefab;

        // 2. Где спавнить?
        Transform start = GetStartPosition();           // Mirror найдёт случайную точку
        Vector3 pos      = start ? start.position  : Vector3.zero;
        Quaternion rot   = start ? start.rotation  : Quaternion.identity;

        // 3. Инстанцируем в нужной координате
        return Instantiate(prefab, pos, rot);
    }

}

