using System;
using Unity.Cinemachine;
using UnityEngine;

public class ChangeAnimal : MonoBehaviour
{
    public GameObject[] animals;
    public PlayerMovement playerMovement;
    private CinemachineOrbitalFollow cameraOrbital;
    private CinemachineRotationComposer cameraRotationComposer;

    private void Start()
    {
        
        cameraRotationComposer = playerMovement.camera.GetComponent<CinemachineRotationComposer>();
        cameraOrbital = playerMovement.camera.GetComponent<CinemachineOrbitalFollow>();
        SetActiveAnimal(1);
    }

    private void Update()
    {
        for (int i = 0; i < animals.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetActiveAnimal(i);
            }
        }
    }

    private void SetActiveAnimal(int index)
    {
        // отключаем всех
        foreach (var animal in animals)
        {
            animal.SetActive(false);
        }

        // включаем выбранного
        animals[index].SetActive(true);

        var animalSettings = animals[index].GetComponent<AnimalSettings>();
        
        
        // меняем аниматор у PlayerMovement
        /*playerMovement.controller.height = animalSettings.colliderHeight;
        playerMovement.controller.radius = animalSettings.colliderRadius;
        playerMovement.controller.center = animalSettings.colliderOffset;*/
        cameraOrbital.TargetOffset = animalSettings.targetOffset;
        cameraOrbital.VerticalAxis.Value = animalSettings.cameraHeight;
        cameraRotationComposer.Composition.ScreenPosition = animalSettings.screenPosition;
        playerMovement.runSpeed = animalSettings.runSpeed;
        playerMovement.walkSpeed = animalSettings.walkSpeed;
        
        playerMovement.animator = animals[index].GetComponent<Animator>();
    }
}