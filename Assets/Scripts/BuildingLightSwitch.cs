using System.Collections.Generic;
using UnityEngine;

public class BuildingLightSwitch : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer buildingRenderer;
    public Sprite offSprite;
    public Sprite onSprite;

    [Header("Settings")]
    public float powerThreshold = 50f;
    public string requiredTag = "PowerPath"; // Optional: tag for related anchors

    private bool isOn = false;
    private List<WireAnchor> dynamicAnchors;

    void Start()
    {
        dynamicAnchors = new List<WireAnchor>();

        foreach (var anchor in FindObjectsOfType<WireAnchor>())
        {
            if (anchor.CompareTag(requiredTag))  // "PowerPath"
            {
                dynamicAnchors.Add(anchor);
            }
        }

        if (dynamicAnchors.Count == 0)
        {
            Debug.LogWarning("❗ No anchors with tag '" + requiredTag + "' found for " + gameObject.name);
        }

        int powerPathCount = 0;
        int turbineAnchorCount = 0;

        foreach (var anchor in FindObjectsOfType<WireAnchor>())
        {
            if (anchor.CompareTag("PowerPath"))
                powerPathCount++;

            if (anchor.CompareTag("TurbineAnchor"))
                turbineAnchorCount++;
        }
    }

    public void UpdateLight(double currentPower)
    {
        bool shouldTurnOn = currentPower >= powerThreshold && IsConnectedToTurbine() && AreAllAnchorsConnected();

        if (shouldTurnOn != isOn)
        {
            isOn = shouldTurnOn;     

            if (buildingRenderer != null)
            {
                if (isOn)
                {
                    StartCoroutine(FlickerOnEffect());
                }
                else
                {
                    buildingRenderer.sprite = offSprite;
                }  
            }
            else
            {
                Debug.LogWarning("❗ buildingRenderer is NOT assigned!");
            }
        }
    }

    public static void RefreshAllBuildingLights(double currentPower)
    {
        foreach (var building in FindObjectsOfType<BuildingLightSwitch>())
        {
            building.UpdateLight(currentPower);
        }
    }


    private bool AreAllAnchorsConnected()
    {
        foreach (var anchor in dynamicAnchors)
        {
            if (!anchor.isConnected)
                return false;
        }
        return true;
    }

    private bool IsConnectedToTurbine()
    {
        Queue<WireAnchor> queue = new Queue<WireAnchor>();
        HashSet<WireAnchor> visited = new HashSet<WireAnchor>();

        // 🟢 Start with anchors assigned to this building
        foreach (var anchor in dynamicAnchors)
        {
            queue.Enqueue(anchor);
            visited.Add(anchor);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.isConnected && current.connectedWire != null)
            {
                foreach (var neighbor in FindConnectedAnchors(current.connectedWire))
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (neighbor.CompareTag("TurbineAnchor")) // ✅ You can tag turbine anchors
                        {
                            return true;
                        }

                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }

        return false;
    }

    private List<WireAnchor> FindConnectedAnchors(GameObject wire)
    {
        List<WireAnchor> connectedAnchors = new List<WireAnchor>();
        foreach (var anchor in FindObjectsOfType<WireAnchor>())
        {
            if (anchor.connectedWire == wire)
                connectedAnchors.Add(anchor);
        }
        return connectedAnchors;
    }

    public void RefreshAnchors()
    {
        dynamicAnchors = new List<WireAnchor>();

        foreach (var anchor in FindObjectsOfType<WireAnchor>())
        {
            if (anchor.CompareTag(requiredTag))  // "PowerPath"
            {
                dynamicAnchors.Add(anchor);
            }
        }
    }

    private System.Collections.IEnumerator FlickerOnEffect()
    {
        int flickerCount = 3;
        float flickerInterval = 0.1f;

        for (int i = 0; i < flickerCount; i++)
        {
            if (buildingRenderer != null)
            {
                buildingRenderer.sprite = offSprite;
            }
            yield return new WaitForSeconds(flickerInterval);

            if (buildingRenderer != null)
            {
                buildingRenderer.sprite = onSprite;
                AudioManager.Instance.PlayFlicker();
            }
            yield return new WaitForSeconds(flickerInterval);
        }

        // Final state is ON
        if (buildingRenderer != null)
        {
            buildingRenderer.sprite = onSprite;
            AudioManager.Instance.PlayFlicker();
        }
    }

}
