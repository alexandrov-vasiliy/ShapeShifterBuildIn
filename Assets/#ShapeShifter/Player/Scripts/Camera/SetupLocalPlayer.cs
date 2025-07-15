using Mirror;
using UnityEngine;

public class SetupLocalPlayer : NetworkBehaviour
{
    public GameObject playerCameras;
    public GameObject hudCanvas;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            // отключаем камеру у чужих игроков
            playerCameras.SetActive(false);
            hudCanvas.SetActive(false);
        }
        else
        {
            // тут можно настроить локального игрока, например включить HUD
            playerCameras.SetActive(true);
            hudCanvas.SetActive(true);
        }
    }
}