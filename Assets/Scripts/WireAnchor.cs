using UnityEngine;

public class WireAnchor : MonoBehaviour
{   
    public bool isConnected = false;
    public GameObject connectedWire = null;

    private void OnMouseDown()
    {
        if (!isConnected)
        {
            WireManager.Instance.StartCableFrom(this);
        }
    }
}
