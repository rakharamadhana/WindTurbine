using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;         // Looping background music
    public AudioSource seaBgmSource;    // Looping ambient wind sound
    public AudioSource windLoopSource;    // Looping ambient wind sound
    public AudioSource sfxSource;         // For one-shot sound effects

    [Header("Wind Loop Volume Settings")]
    public float minWindVolume = 0.1f;
    public float maxWindVolume = 1.0f;

    [Header("Wind Particle Clip")]
    public AudioClip windParticleClip;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    [Header("Building Particle")]
    public AudioClip buildingFlickerClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM()
    {
        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    public void PlaySeaBGM()
    {
        if (!seaBgmSource.isPlaying)
            seaBgmSource.Play();
    }

    public void PlayWindLoop()
    {
        if (!windLoopSource.isPlaying)
            windLoopSource.Play();
    }

    public void UpdateWindVolume(float windSpeed, float maxSpeed)
    {
        float t = Mathf.Clamp01(windSpeed / maxSpeed);
        float volume = Mathf.Lerp(minWindVolume, maxWindVolume, t);
        windLoopSource.volume = volume;
    }

    public void PlayWindParticle()
    {
        GameObject temp = new GameObject("TempWindSFX");
        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = windParticleClip;
        source.pitch = Random.Range(minPitch, maxPitch); // 🎯 Randomize pitch
        source.spatialBlend = 0f; // Set to 1f if you want 3D sound
        source.Play();

        Destroy(temp, windParticleClip.length / source.pitch); // Clean up after play
    }

    public void PlayFlicker()
    {
        sfxSource.PlayOneShot(buildingFlickerClip);
    }
}
