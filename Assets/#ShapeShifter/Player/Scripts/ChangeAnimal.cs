using System;
using Mirror;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Allows the owner of a player object to switch between several animal meshes/animators.
/// A non‑owner can no longer trigger the change because:
///  1. Update() runs only for the client that has authority.
///  2. The Command has <see cref="CommandAttribute.requiresAuthority"/> = true (default)
///     *and* additionally checks <paramref name="sender"/>.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class ChangeAnimal : NetworkBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject deadModel;

    [SerializeField] private GameObject[] animals = Array.Empty<GameObject>();
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private NetworkAnimator networkAnimator;


    // Camera components are optional – keep them if you really use them somewhere else.
    private CinemachineOrbitalFollow cameraOrbital;
    private CinemachineRotationComposer cameraRotationComposer;

    [Header("Runtime state")] // Visible in the inspector at runtime only
    [SyncVar(hook = nameof(OnAnimalIndexChanged))]
    private int activeAnimalIndex = 0;

    public bool isDead = false;

    #region Unity callbacks

    private void Awake()
    {
        // Cache camera components once so we don't search every time.
        cameraOrbital = Camera.main?.GetComponent<CinemachineOrbitalFollow>();
        cameraRotationComposer = Camera.main?.GetComponent<CinemachineRotationComposer>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Ensure the correct model is active when we connect late.
        SetActiveAnimal(activeAnimalIndex);
    }

    private void Update()
    {
        // Make sure **only** the owner handles input.
        if (!isOwned || isDead) return;

        for (int i = 0; i < animals.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i != activeAnimalIndex)
            {
                // Local validation so we don't spam the server with invalid data.
                if (i < 0 || i >= animals.Length) continue;
                CmdRequestChangeAnimal(i);
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------------
    //                            Server side
    // ---------------------------------------------------------------------


    public void SetDeadState()
    {
        isDead = true;

        // Отключаем все животные модели
        foreach (var animal in animals)
        {
            if (animal != null)
                animal.SetActive(false);
        }

        // Включаем мёртвую модель, если задана
        if (deadModel != null)
            deadModel.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Можно отключить управление, если хочешь:
        if (playerMovement != null)
            playerMovement.enabled = false;
    }


    /// <summary>
    /// Executed on the *server*.
    /// The extra <paramref name="sender"/> parameter is filled automatically by Mirror and
    /// lets us verify that the caller really owns this object.
    /// </summary>
    [Command(requiresAuthority = true)]
    private void CmdRequestChangeAnimal(int index, NetworkConnectionToClient sender = null)
    {
        // SECURITY: If for some reason a client that does *not* own this object gained authority
        // over it (mis‑configuration) we block the request here.
        if (sender != connectionToClient) return;

        if (index < 0 || index >= animals.Length) return; // extra sanity check

        activeAnimalIndex = index; // Triggers the SyncVar hook on every client
    }

    // ---------------------------------------------------------------------
    //                            Client side
    // ---------------------------------------------------------------------

    /// <summary>
    /// Called automatically on *all* clients when <see cref="activeAnimalIndex"/> changes.
    /// </summary>
    private void OnAnimalIndexChanged(int oldIndex, int newIndex) => SetActiveAnimal(newIndex);

    /// <summary>
    /// Enables the selected animal mesh, updates movement stats and animator references.
    /// Executes on both server and clients.
    /// </summary>
    private void SetActiveAnimal(int index)
    {
        if (index < 0 || index >= animals.Length)
        {
            Debug.LogWarning($"[ChangeAnimal] Invalid animal index {index} on {name}");
            return;
        }

        // Activate chosen model / deactivate the rest
        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].SetActive(i == index);
        }

        ApplyStatsAndAnimator(index);
    }

    private void ApplyStatsAndAnimator(int index)
    {
        if (!playerMovement) return;

        if (animals[index].TryGetComponent(out AnimalSettings animalSettings))
        {
            playerMovement.runSpeed = animalSettings.runSpeed;
            playerMovement.walkSpeed = animalSettings.walkSpeed;
        }

        if (animals[index].TryGetComponent(out Animator animator))
        {
            animator.SetFloat("Speed_f", 0f);
            networkAnimator.animator = animator;
            playerMovement.animator = animator;
        }
    }
}