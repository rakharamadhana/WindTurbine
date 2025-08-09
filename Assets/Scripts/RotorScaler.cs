using UnityEngine;

public class RotorScaler : MonoBehaviour
{
    public UniformScaleController scaleController; // ✅ Reference to your controller
    public float minDiameter = 110f;
    public float maxDiameter = 140f;

    private Vector3 lastMousePos;
    private bool isDragging = false;
    private float currentDiameter = 110f;

    void Start()
    {
        if (scaleController == null)
        {
            scaleController = FindObjectOfType<UniformScaleController>();
        }

        if (scaleController == null)
        {
            Debug.LogWarning("❗ No UniformScaleController found in the scene.");
        }

        currentDiameter = minDiameter;
    }


    void OnMouseDown()
    {
        isDragging = true;
        lastMousePos = Input.mousePosition;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (!isDragging || scaleController == null) return;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            float deltaValue = delta.x + delta.y;
            float sensitivity = 0.1f;

            currentDiameter += deltaValue * sensitivity;
            currentDiameter = Mathf.Clamp(currentDiameter, minDiameter, maxDiameter);

            scaleController.UpdateScaleFromDiameter(currentDiameter);

            lastMousePos = Input.mousePosition;
        }
    }
}
