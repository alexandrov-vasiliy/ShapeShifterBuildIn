using System;
using Mirror;
using Unity.Cinemachine;
using UnityEngine;

public class ChangeAnimal : NetworkBehaviour
{
    public GameObject[] animals;
    public PlayerMovement playerMovement;
    private CinemachineOrbitalFollow cameraOrbital;
    private CinemachineRotationComposer cameraRotationComposer;
    public NetworkAnimator networkAnimator;

    [SyncVar(hook = nameof(OnAnimalIndexChanged))]
    private int activeAnimalIndex = 2;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetActiveAnimal(activeAnimalIndex);
    }


    private void Update()
    {
        if (!isLocalPlayer) return;

        for (int i = 0; i < animals.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                CmdChangeAnimal(i);
            }
        }
    }

    [Command]
    private void CmdChangeAnimal(int index)
    {
        // Меняем SyncVar на сервере - Mirror автоматически вызовет hook на всех клиентах
        activeAnimalIndex = index;
    }

    // Этот hook вызовется автоматически у всех клиентов
    private void OnAnimalIndexChanged(int oldIndex, int newIndex)
    {
        SetActiveAnimal(newIndex);
    }


    private void SetActiveAnimal(int index)
    {
        if (index < 0 || index >= animals.Length)
        {
            Debug.LogWarning($"Invalid animal index {index}");
            return;
        }

        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].SetActive(i == index);
        }

        var animalSettings = animals[index].GetComponent<AnimalSettings>();
        playerMovement.runSpeed = animalSettings.runSpeed;
        playerMovement.walkSpeed = animalSettings.walkSpeed;
        var animator = animals[index].GetComponent<Animator>();
        animator.SetFloat("Speed_f", 0);
        networkAnimator.animator = animator;
        playerMovement.animator = animator;
    }
}