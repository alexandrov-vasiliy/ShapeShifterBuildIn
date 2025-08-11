using UnityEngine;
using System.Collections;

public class ImpactScript : MonoBehaviour {

	[Header("Impact Despawn Timer")]
	//How long before the impact is destroyed
	public float despawnTimer = 10.0f;

	[Header("Audio")]
	public AudioClip[] impactSounds;
	public AudioSource audioSource;

	private void Start () {

		
		audioSource.clip = impactSounds
			[Random.Range(0, impactSounds.Length)];
		audioSource.Play();
			
		Destroy (gameObject, despawnTimer);

	}
}