using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class MobStepSound : MonoBehaviour
{
    public AudioClip[] stepSounds;

    public AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void StepSoundPlay()
    {
        if(stepSounds.Length == 0) return;
        _audioSource.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)]);
        Debug.Log("step sound");
    }
}
