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

    [Command]
    private void CmdSpawnProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject projectile = Instantiate(projectilePrefab, position, rotation);
        projectile.GetComponent<Rigidbody>().linearVelocity = projectile.transform.forward * projectileImpulse;
        NetworkServer.Spawn(projectile);
    }
}