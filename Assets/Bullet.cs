using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
   public ParticleSystem Muzzle;
   public ParticleSystem Tracer;

   private void Start()
   {
      Muzzle.Play();
      //Tracer.Play();
      Destroy(gameObject, 1f);
   }
}
