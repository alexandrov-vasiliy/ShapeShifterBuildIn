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

    [SyncVar(hook = nameof(OnAnimalIndexChanged))]
    private int activeAnimalIndex = 2;

    private void Start()
    {
        cameraRotationComposer = playerMovement.camera.GetComponent<CinemachineRotationComposer>();
        cameraOrbital = playerMovement.camera.GetComponent<CinemachineOrbitalFollow>();

        // Применяем модель при старте (для вновь подключившихся клиентов)
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
        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].SetActive(i == index);
        }

        var animalSettings = animals[index].GetComponent<AnimalSettings>();

        cameraOrbital.TargetOffset = animalSettings.targetOffset;
        cameraOrbital.VerticalAxis.Value = animalSettings.cameraHeight;
        cameraRotationComposer.Composition.ScreenPosition = animalSettings.screenPosition;
        playerMovement.runSpeed = animalSettings.runSpeed;
        playerMovement.walkSpeed = animalSettings.walkSpeed;
        
        playerMovement.animator = animals[index].GetComponent<Animator>();
    }
}
