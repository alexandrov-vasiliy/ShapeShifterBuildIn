using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")] 
    public float walkSpeed;

    public float runSpeed;
    [SerializeField] private float gravity = -10f;
    [SerializeField] private float smoothTime;
    [SerializeField] private float smooth;
    public Transform firstCamera;
    public float currentSpeed;
    
    [Header("References")]
    public CharacterController controller;
    public Animator animator;
    public CinemachineCamera camera;


    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private bool isFrozen;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Update()
    {
        if (!isFrozen)
        {
            HandleMovement();
        }
        HandleGravity();
    }


    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        float animSpeedValue = Input.GetKey(KeyCode.LeftShift) ? 1f : 0.5f;
        if (horizontal != 0 || vertical != 0)
        {
            animator.SetFloat("Speed_f", animSpeedValue);
        }
        else
        {
            animator.SetFloat("Speed_f", 0f);
        }

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            float rotationAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg +
                                  firstCamera.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref smooth, smoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 move = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
            controller.Move(move.normalized * currentSpeed * Time.deltaTime + Vector3.up * verticalVelocity);
        }
    }

    private void HandleGravity()
    {
        // Gravity
        if (!controller.isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = -0.1f;
        }
    }

    public void Freeze(float duration)
    {
        StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        animator.SetFloat("Speed_f", 0f); 
        yield return new WaitForSeconds(duration);
        animator.SetBool("Eat_b", false);
        isFrozen = false;
    }
    
    
}