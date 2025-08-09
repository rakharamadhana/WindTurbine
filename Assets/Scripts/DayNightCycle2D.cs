using UnityEngine;
using UnityEngine.Rendering.Universal; // For Light2D

public class DayNightCycle2D : MonoBehaviour
{
    public Light2D globalLight;       // Assign your Global Light 2D here
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.05f, 0.05f, 0.2f); // Dark blue

    [Range(0f, 1f)] public float intensityDay = 1f;
    [Range(0f, 1f)] public float intensityNight = 0.2f;

    public float cycleDuration = 10f; // Seconds per full cycle (Day→Night→Day)

    private float timer = 0f;

    void Update()
    {
        if (globalLight == null) return;

        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer / (cycleDuration / 2f), 1f);

        // Interpolate color and intensity
        globalLight.color = Color.Lerp(dayColor, nightColor, t);
        globalLight.intensity = Mathf.Lerp(intensityDay, intensityNight, t);
    }

    public void SetDay()
    {
        globalLight.color = dayColor;
        globalLight.intensity = intensityDay;
    }

    public void SetNight()
    {
        globalLight.color = nightColor;
        globalLight.intensity = intensityNight;
    }

}
