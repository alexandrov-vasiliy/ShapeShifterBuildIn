using Mirror;
using UnityEngine;

public class WeaponNetwork : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileImpulse;

    public void SpawnNetworkProjectile(Vector3 position, Quaternion rotation)
    {
        CmdSpawnProjectile(position, rotation);
    }

    [Command/*(requiresAuthority = false)*/]
    private void CmdSpawnProjectile(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(projectilePrefab, position, rotation);
        
        var bulletCol = go.GetComponent<Collider>();
        if (bulletCol != null && connectionToClient?.identity != null)
        {
            foreach (var col in connectionToClient.identity.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(bulletCol, col, true);
        }

        
        var rb = go.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;
        rb.linearVelocity  = go.transform.forward * projectileImpulse; // <-- гарантированная скорость

        NetworkServer.Spawn(go);
    }
}