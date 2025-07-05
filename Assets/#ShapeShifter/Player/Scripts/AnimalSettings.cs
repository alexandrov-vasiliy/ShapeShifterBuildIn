using System;
using UnityEngine;

public class AnimalSettings : MonoBehaviour
{
    public float cameraHeight;
    public float colliderHeight;
    public Vector3 colliderOffset;
    public float colliderRadius;
    public Vector3 targetOffset;
    public Vector2 screenPosition;
    public float runSpeed;
    public float walkSpeed;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        Gizmos.DrawWireSphere(gameObject.transform.position + colliderOffset, colliderRadius);
        
    }
}