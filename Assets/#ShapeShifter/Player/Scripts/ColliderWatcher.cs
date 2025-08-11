using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class ColliderWatcher : MonoBehaviour
{
    public UnityEvent onBulletEnter;
    private bool isDead;

    [ServerCallback] // <- важно: триггер обрабатывается только на сервере
    private void OnTriggerEnter(Collider other)
    {

        if (isDead) return;
        if (other.GetComponent<bulletName>())
        {
            Debug.Log("EnterBullet"+ " " + name);
            isDead = true;
            onBulletEnter.Invoke(); // это будет выполняться на сервере
        }
    }
}
