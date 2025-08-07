using Mirror;
using UnityEngine;

public class DeadSystem : NetworkBehaviour
{
    public ChangeAnimal changeAnimal;
    [SerializeField] private GameObject modelToDisable;
    private bool isDead = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        // Только реагировать, если еще жив
        if (isDead) return;

        if (other.GetComponent<bulletName>())
        {
            isDead = true;
            RpcDie();
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (changeAnimal != null)
            changeAnimal.SetDeadState();
    }
}
