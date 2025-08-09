using UnityEngine;

public class WindParticleController : MonoBehaviour
{
    public WindZone windZone;                // Reference your 2D wind speed source
    public ParticleSystem windParticles;     // Drag your particle system here
    public float speedMultiplier = 1f;       // Adjust visual exaggeration

    private ParticleSystem.MainModule mainModule;

    void Start()
    {
        if (windParticles != null)
            mainModule = windParticles.main;
    }

    void Update()
    {
        if (windZone != null && windParticles != null)
        {
            float windSpeed = windZone.windMain;
            mainModule.startSpeed = windSpeed * speedMultiplier; // e.g., 1x visual speed
        }
    }
}
