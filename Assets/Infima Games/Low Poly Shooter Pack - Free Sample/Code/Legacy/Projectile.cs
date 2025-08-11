using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Mirror;
using Random = UnityEngine.Random;

public class Projectile : NetworkBehaviour {

	[Range(5, 100)]
	[Tooltip("After how long time should the bullet prefab be destroyed?")]
	public float destroyAfter;
	[Tooltip("If enabled the bullet destroys on impact")]
	public bool destroyOnImpact = false;
	[Tooltip("Minimum time after impact that the bullet is destroyed")]
	public float minDestroyTime;
	[Tooltip("Maximum time after impact that the bullet is destroyed")]
	public float maxDestroyTime;

	[Header("Impact Effect Prefabs")]
	public Transform [] bloodImpactPrefabs;
	public Transform [] metalImpactPrefabs;
	public Transform [] dirtImpactPrefabs;
	public Transform []	concreteImpactPrefabs;

	private void Start ()
	{
		//Grab the game mode service, we need it to access the player character!
		var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
		//Ignore the main player character's collision. A little hacky, but it should work.
		Physics.IgnoreCollision(gameModeService.GetPlayerCharacter().GetComponent<Collider>(), GetComponent<Collider>());
		
		if (isServer) StartCoroutine(DestroyAfter());
		
	}


	[ServerCallback]
	private void OnCollisionEnter (Collision collision)
	{
		//Ignore collisions with other projectiles.
		if (collision.gameObject.GetComponent<Projectile>() != null)
			return;
		Debug.Log("Bullet Collision " + collision.transform.tag);
		
		var contact = collision.GetContact(0);
		string surface = collision.transform.tag;

		RpcImpactEffect(contact.point, contact.normal, surface);
		StartCoroutine(DestroyNextFrame());


	}

	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out HitBox hb))
		{
			Debug.Log($"Collided in hitbox {hb.name}");
			hb.ApplyHit();
			Vector3 point = other.ClosestPoint(transform.position);
			Vector3 normal = -GetComponent<Rigidbody>().linearVelocity.normalized;
			string surface = "Blood"; 
			RpcImpactEffect(point, normal, surface);
			
			StartCoroutine(DestroyNextFrame());


		}
	}


	[ClientRpc]
	public void RpcImpactEffect(Vector3 point, Vector3 normal, string surface)
	{
		Transform fx = null;
		Debug.Log($"Spawn impact surface: {surface}");
		switch (surface)
		{
			case "Player":
				if (bloodImpactPrefabs?.Length > 0)
					fx = bloodImpactPrefabs[Random.Range(0, bloodImpactPrefabs.Length)];
				break;
			case "Blood":
				if (bloodImpactPrefabs?.Length > 0)
					fx = bloodImpactPrefabs[Random.Range(0, bloodImpactPrefabs.Length)];
				break;
			case "Metal":
				if (metalImpactPrefabs?.Length > 0)
					fx = metalImpactPrefabs[Random.Range(0, metalImpactPrefabs.Length)];
				break;
			case "Dirt":
				if (dirtImpactPrefabs?.Length > 0)
					fx = dirtImpactPrefabs[Random.Range(0, dirtImpactPrefabs.Length)];
				break;
			case "Concrete":
				if (concreteImpactPrefabs?.Length > 0)
					fx = concreteImpactPrefabs[Random.Range(0, concreteImpactPrefabs.Length)];
				break;
		}
		Debug.Log($"Spawn impact fx: {fx.name}");
		if (fx != null) Instantiate(fx, point, Quaternion.LookRotation(normal));
	}
	
	IEnumerator DestroyNextFrame(){
		yield return new WaitForEndOfFrame();
		NetworkServer.Destroy(gameObject); 
	}
	
	private IEnumerator DestroyAfter()
	{
		yield return new WaitForSeconds(destroyAfter);
		if (isServer) NetworkServer.Destroy(gameObject);
	}
	
}