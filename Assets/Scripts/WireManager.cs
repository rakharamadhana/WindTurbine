using UnityEngine;

public class WireManager : MonoBehaviour
{
    public static WireManager Instance;

    public GameObject wirePrefab;     // Prefab with LineRenderer
    public GameObject arrowPrefab;    // Arrow prefab with ArrowAlongWire script

    private WireAnchor startAnchor;
    private LineRenderer currentLine;
    private WireAnchor hoveredAnchor = null;
    private bool waitingForSecondClick = false;

    private readonly Color defaultColor = Color.black;
    private readonly Color hoverColor = Color.green;

    void Awake()
    {
        Instance = this;
    }

    public void StartCableFrom(WireAnchor anchor)
    {
        if (currentLine != null) return;

        startAnchor = anchor;
        waitingForSecondClick = false;

        GameObject wireObj = Instantiate(wirePrefab);
        currentLine = wireObj.GetComponent<LineRenderer>();
        currentLine.positionCount = 2;

        Vector3 startPos = anchor.transform.position;
        startPos.z = 0f;
        currentLine.SetPosition(0, startPos);

        Vector3 mousePos = GetMouseWorldPosition();
        mousePos.z = 0f;
        currentLine.SetPosition(1, mousePos);

        StartCoroutine(EnableClickDetectionNextFrame());
    }

    private System.Collections.IEnumerator EnableClickDetectionNextFrame()
    {
        yield return null;
        waitingForSecondClick = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && currentLine == null)
        {
            TryDeleteWireAtMouse();
        }

        if (currentLine != null && waitingForSecondClick)
        {
            HandleWireDrawing();
        }
    }

    private void HandleWireDrawing()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        mousePos.z = 0f;
        currentLine.SetPosition(1, mousePos);

        // Hover detection
        hoveredAnchor = null;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            WireAnchor anchor = hit.collider.GetComponentInParent<WireAnchor>();
            if (anchor != null && anchor != startAnchor && !anchor.isConnected)
            {
                hoveredAnchor = anchor;
                currentLine.startColor = hoverColor;
                currentLine.endColor = hoverColor;
            }
            else
            {
                currentLine.startColor = defaultColor;
                currentLine.endColor = defaultColor;
            }
        }
        else
        {
            currentLine.startColor = defaultColor;
            currentLine.endColor = defaultColor;
        }

        // Left-click to confirm connection
        if (Input.GetMouseButtonDown(0))
        {
            if (hoveredAnchor != null)
            {
                CompleteWireConnection();
            }
            else
            {
                CancelWireConnection();
            }
        }

        // Right-click to cancel wire creation
        if (Input.GetMouseButtonDown(1))
        {
            CancelWireConnection();
        }
    }

    private void CompleteWireConnection()
    {
        Vector3 endPos = hoveredAnchor.transform.position;
        endPos.z = 0f;
        currentLine.SetPosition(1, endPos);

        startAnchor.isConnected = true;
        hoveredAnchor.isConnected = true;
        startAnchor.connectedWire = currentLine.gameObject;
        hoveredAnchor.connectedWire = currentLine.gameObject;

        // ✨ Spawn multiple arrows
        SpawnMultipleArrows(currentLine, 3);

        // 🔄 Immediate refresh to update existing building light state
        double currentPower = ElectricWire.ElectricWindTurbine.currentPower;

        foreach (var building in FindObjectsOfType<BuildingLightSwitch>())
        {
            building.RefreshAnchors();
            building.UpdateLight(currentPower);
        }

        BuildingLightSwitch.RefreshAllBuildingLights(currentPower);

        // ✅ Delay full recheck in case something lags
        StartCoroutine(DelayedRefresh());

        ResetState();
    }

    private System.Collections.IEnumerator DelayedRefresh()
    {
        yield return null; // Frame 1 — allow wire and anchor state to update

        var turbine = FindObjectOfType<ElectricWire.ElectricWindTurbine>();
        if (turbine != null)
        {
            turbine.SendMessage("ManageEnergy");
        }

        // Wait one more frame to ensure building anchors are ready
        yield return null;

        double currentPower = ElectricWire.ElectricWindTurbine.currentPower;

        foreach (var building in FindObjectsOfType<BuildingLightSwitch>())
        {
            building.RefreshAnchors();
            building.UpdateLight(currentPower);
        }
        BuildingLightSwitch.RefreshAllBuildingLights(currentPower);
    }


    private void CancelWireConnection()
    {
        Destroy(currentLine.gameObject);
        ResetState();
    }

    private void TryDeleteWireAtMouse()
    {
        Vector2 mouseWorld2D = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld2D, Vector2.zero);
        if (hit.collider != null)
        {
            WireAnchor anchor = hit.collider.GetComponentInParent<WireAnchor>();
            if (anchor != null && anchor.isConnected && anchor.connectedWire != null)
            {
                Debug.Log("🗑️ Deleting wire from: " + anchor.name);
                Destroy(anchor.connectedWire);

                foreach (var a in FindObjectsOfType<WireAnchor>())
                {
                    if (a.connectedWire == anchor.connectedWire)
                    {
                        a.isConnected = false;
                        a.connectedWire = null;
                    }
                }
            }
        }
    }

    private void ResetState()
    {
        currentLine = null;
        startAnchor = null;
        hoveredAnchor = null;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = 0f;
        return Camera.main.ScreenToWorldPoint(mouse);
    }

    public void SpawnMultipleArrows(LineRenderer wireLine, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject arrowInstance = Instantiate(arrowPrefab, wireLine.transform);
            ArrowAlongWire arrowScript = arrowInstance.GetComponent<ArrowAlongWire>();

            if (arrowScript != null)
            {
                arrowScript.wireLine = wireLine;
                arrowScript.speed = 1f;
                arrowScript.startOffset = (float)i / count;
                arrowScript.enabled = true;
            }
        }
    }
}
