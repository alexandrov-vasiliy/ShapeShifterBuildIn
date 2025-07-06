using UnityEngine;

public class MobStepSound : MonoBehaviour
{
    public AudioClip[] stepSounds;

    public AudioSource _audioSource;
    

    
    public void StepSoundPlay()
    {
        _audioSource.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)]);
        Debug.Log("step sound");
    }
}
