using UnityEngine;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 300f; // например 5 минут
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
    public TMP_Text timerText;

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

            if (timerText != null)
            {
                float remaining = Mathf.Clamp(dayDuration - currentTime, 0, dayDuration);
                int minutes = Mathf.FloorToInt(remaining / 60f);
                int seconds = Mathf.FloorToInt(remaining % 60);

                timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
            }

            if (currentTime >= dayDuration)
            {
                StartNight();
            }
        }
        else if (isNightRunning)
        {
            // можешь добавить здесь логику для отображения времени на ночь
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

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(dayDuration / 60f);
            int seconds = Mathf.FloorToInt(dayDuration % 60);
            timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
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

        if (timerText != null)
            timerText.text = "Night!";
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
