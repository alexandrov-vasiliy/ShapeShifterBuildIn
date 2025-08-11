using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class HitBox : MonoBehaviour
{
    public UnityEvent onBulletEnter;
    private bool isDead;

    [Server]
    public void ApplyHit()
    {
        Debug.Log("EnterBullet " + name);
        isDead = true;
        onBulletEnter.Invoke();
    }
}
