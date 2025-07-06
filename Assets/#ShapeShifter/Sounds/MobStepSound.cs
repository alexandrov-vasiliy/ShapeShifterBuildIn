using UnityEngine;

public class MobStepSound : MonoBehaviour
{
    public AudioClip[] stepSounds;

    public AudioSource _audioSource;
    

    
    public void StepSoundPlay()
    {
        if(stepSounds.Length == 0) return;
        _audioSource.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)]);
        Debug.Log("step sound");
    }
}
