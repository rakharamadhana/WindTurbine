using UnityEngine;

public class WindSpawner : MonoBehaviour
{
    public GameObject[] windPrefabs; // Assign Wind and Wind 2 in Inspector
    public float spawnInterval = 2f;
    public Vector2 spawnArea = new Vector2(10f, 5f);

    void Start()
    {
        InvokeRepeating(nameof(SpawnWind), 1f, spawnInterval);
    }

    void SpawnWind()
    {
        GameObject prefab = windPrefabs[Random.Range(0, windPrefabs.Length)];

        Vector3 position = transform.position + new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            Random.Range(-spawnArea.y, spawnArea.y),
            0f
        );

        GameObject instance = Instantiate(prefab, position, prefab.transform.rotation);

        // Start coroutine to play SFX after delay
        StartCoroutine(DelayedWindSFX(3f));

        // Normalize wind speed to [2.5, 10.0] range
        float windSpeed = WindUIController.CurrentWindSpeed;
        float maxWind = WindUIController.MaxWindSpeed;
        float velocityMultiplier = Mathf.Lerp(2.5f, 10f, windSpeed / maxWind);


        var ps = instance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;

            vel.x = MultiplyCurve(vel.x, velocityMultiplier);
            vel.y = MultiplyCurve(vel.y, velocityMultiplier);
            vel.z = MultiplyCurve(vel.z, velocityMultiplier);
        }

        Destroy(instance, 10f);
    }

    System.Collections.IEnumerator DelayedWindSFX(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance.PlayWindParticle();
    }

    ParticleSystem.MinMaxCurve MultiplyCurve(ParticleSystem.MinMaxCurve original, float multiplier)
    {
        switch (original.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return new ParticleSystem.MinMaxCurve(original.constant * multiplier);
            case ParticleSystemCurveMode.TwoConstants:
                return new ParticleSystem.MinMaxCurve(original.constantMin * multiplier, original.constantMax * multiplier);
            case ParticleSystemCurveMode.Curve:
                return new ParticleSystem.MinMaxCurve(multiplier, original.curve);
            case ParticleSystemCurveMode.TwoCurves:
                return new ParticleSystem.MinMaxCurve(multiplier, original.curveMin, original.curveMax);
            default:
                return original;
        }
    }
}
