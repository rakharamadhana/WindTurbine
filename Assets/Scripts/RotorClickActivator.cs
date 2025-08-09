using UnityEngine;

public class RotorClickActivator : MonoBehaviour
{
    public GameObject scalerHandle;

    private void OnMouseDown()
    {
        Debug.Log("🌀 Wind turbine clicked!");

        if (scalerHandle != null)
        {
            scalerHandle.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠️ Scaler handle is not assigned.");
        }
    }
}
