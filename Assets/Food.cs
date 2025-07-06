using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Outline outline;
    public ParticleSystem deadParticle;
    public GameObject visual;

    private void Start()
    {
        outline.enabled = false;
    }

    public void Eat()
    {
        StartCoroutine(EatRoutine());
    }

    private IEnumerator EatRoutine()
    {
        visual.SetActive(false);
        deadParticle.Play();
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }
}