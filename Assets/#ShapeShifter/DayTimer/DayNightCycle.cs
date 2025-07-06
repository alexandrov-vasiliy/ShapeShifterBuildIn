using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 30f;
    public Light sun;
    public Color dayColor = Color.white;
    public Color nightColor = Color.blue;

    [Header("Skyboxes")]
    public Material daySkybox;
    public Material nightSkybox;

    [Header("Ambient Lighting")]
    public float dayAmbientIntensity = 1f;
    public float nightAmbientIntensity = 0.2f;

    [Header("UI")]
    public Image timerBar; // <-- сюда в инспекторе перетащим твой Image

    private float currentTime = 0f;
    private bool isDay = true;
    private bool isNightRunning = false;

    void Start()
    {
        StartDay();
    }

    void Update()
    {
        if (isDay)
        {
            currentTime += Time.deltaTime;

            float t = currentTime / dayDuration;

            sun.color = Color.Lerp(dayColor, nightColor, t);
            sun.intensity = Mathf.Lerp(1f, 0.2f, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(dayAmbientIntensity, nightAmbientIntensity, t);

            // обновляем таймер UI
            if (timerBar != null)
                timerBar.fillAmount = Mathf.Clamp01(1f - t);

            if (currentTime >= dayDuration)
            {
                StartNight();
            }
        }
        else if (isNightRunning)
        {
            // можем пустить анимацию таймера на выполнение задачи ночью
        }
    }

    private void StartDay()
    {
        Debug.Log("День начался");
        currentTime = 0f;
        isDay = true;
        isNightRunning = false;

        sun.intensity = 1f;
        sun.color = dayColor;
        RenderSettings.ambientIntensity = dayAmbientIntensity;

        RenderSettings.skybox = daySkybox;
        DynamicGI.UpdateEnvironment();

        if (timerBar != null)
            timerBar.fillAmount = 1f; // день начинается — полная шкала
    }

    private void StartNight()
    {
        Debug.Log("Наступила ночь!");
        isDay = false;
        isNightRunning = true;

        sun.intensity = 0.2f;
        sun.color = nightColor;
        RenderSettings.ambientIntensity = nightAmbientIntensity;

        RenderSettings.skybox = nightSkybox;
        DynamicGI.UpdateEnvironment();

        if (timerBar != null)
            timerBar.fillAmount = 0f; // ночь наступила — шкала пустая
    }

    public void TaskCompleted()
    {
        Debug.Log("Игроки выполнили задачу ночью!");
        isNightRunning = false;
        StartDay();
    }

    public void EndGame()
    {
        Debug.Log("Игра закончена!");
    }
}

