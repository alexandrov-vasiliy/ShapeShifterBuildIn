using Mirror;
using UnityEngine;

public class DeadSystem : NetworkBehaviour
{
    public ChangeAnimal changeAnimal;
    [SerializeField] private GameObject modelToDisable;
    public ColliderWatcher[] watchers;

    private void Start()
    {
        foreach (var watcher in watchers)
        {
            watcher.onBulletEnter.AddListener(Die);
            Debug.Log(watcher.name + "subscribed");
        }
    }


    [ClientRpc]
    void RpcDie()
    {
        if (changeAnimal != null)
            changeAnimal.SetDeadState();
    }


    [Server]
    public void Die()
    {
        RpcDie();
    }
}