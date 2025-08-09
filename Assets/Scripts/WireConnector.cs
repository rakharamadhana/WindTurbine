using UnityEngine;

public class WireConnector : MonoBehaviour
{
    public GameObject arrowPrefab; // Drag your prefab here in Inspector

    public void ConnectWire(GameObject wireObject)
    {
        LineRenderer wireLine = wireObject.GetComponent<LineRenderer>();
        ArrowAlongWire arrowScript = wireObject.GetComponent<ArrowAlongWire>();

        if (wireLine == null || arrowScript == null)
        {
            Debug.LogWarning("❌ Wire missing components");
            return;
        }

        // Instantiate the arrow
        GameObject arrowInstance = Instantiate(arrowPrefab);
        arrowInstance.transform.position = wireLine.GetPosition(0); // start point
        arrowInstance.SetActive(true);

        // Assign it to the script
        arrowScript.wireLine = wireLine;
        arrowScript.enabled = true;
    }
}
