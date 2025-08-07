using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;

/// <summary>
/// Повесьте в *игровую* сцену. Работает только на сервере (хосте).
/// </summary>
public class RandomHunterReplacer : MonoBehaviour
{

    [SerializeField] GameObject survivorPrefab;
    [SerializeField] GameObject hunterPrefab;

    void Start()
    {
        // чтобы не дублироваться на клиентах
        if (!NetworkServer.active) { enabled = false; return; }

        StartCoroutine(ReplaceOnceEveryoneSpawned());
    }

    IEnumerator ReplaceOnceEveryoneSpawned()
    {
        // 1. ждём, пока в gameplay‑сцене у всех соединений появится player‑объект
        yield return new WaitUntil(AllPlayersHaveIdentity);

        // 2. собираем соединения = будущие участники матча
        var conns = NetworkServer.connections.Values
                                            .Where(c => c != null && c.identity != null)
                                            .ToList();
        if (conns.Count == 0) yield break;

        int hunterIndex = Random.Range(0, conns.Count);

        // 3. для каждого: готовим новый объект и подменяем
        for (int i = 0; i < conns.Count; i++)
        {
            var conn      = conns[i];
            var oldPlayer = conn.identity.gameObject;

            GameObject prefab = (i == hunterIndex) ? hunterPrefab : survivorPrefab;
            GameObject newObj = Instantiate(prefab);

            // KeepAuthority = true → клиент сохраняет владение ‒ Mirror документация :contentReference[oaicite:0]{index=0}
            NetworkServer.ReplacePlayerForConnection(conn, newObj, true);
            Destroy(oldPlayer, 0.1f);   // аккуратно удаляем старый объект
        }

        Debug.Log($"[Role] Hunter = connection #{conns[hunterIndex].connectionId}");
        enabled = false;               // скрипт больше не нужен
    }

    bool AllPlayersHaveIdentity()
    {
        // allPlayersReady уже true → все перешли в игровую сцену :contentReference[oaicite:1]{index=1}
        foreach (var c in NetworkServer.connections.Values)
            if (c == null || c.identity == null) return false;
        return true;
    }
}
