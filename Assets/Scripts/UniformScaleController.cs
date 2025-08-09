using UnityEngine;
using TMPro;

public class UniformScaleController : MonoBehaviour
{
    public GameObject targetObject;
    public TMP_InputField rotorInput;        // ✅ Numeric input
    public TextMeshProUGUI scaleText;
    public TextMeshProUGUI rotorSizeText;

    private const float baseDiameter = 126f;
    public float displayToRealScaleRatio = 1.5f;

    public float realScale = 1f; // ✅ Other scripts read this

    [Header("Rotor Limits")]
    public float minRotor = 110f;
    public float maxRotor = 140f;

    [Header("Step Controls")]
    public float step = 10f;   // ✅ Step per button press
    void Start()
    {
        if (rotorInput != null)
        {
            rotorInput.text = minRotor.ToString("0.00");
            UpdateScaleFromInput(rotorInput.text);

            // Use onEndEdit so typing isn't fought while user is editing
            rotorInput.onEndEdit.AddListener(UpdateScaleFromInput);
        }
    }

    void UpdateScaleFromInput(string input)
    {
        if (!float.TryParse(input, out float rotorDiameter))
            return;

        float clampedDiameter = Mathf.Clamp(rotorDiameter, minRotor, maxRotor);
        rotorInput.text = clampedDiameter.ToString("0.00");

        ApplyScale(clampedDiameter);
    }

    public void UpdateScaleFromDiameter(float rotorDiameter)
    {
        float clampedDiameter = Mathf.Clamp(rotorDiameter, minRotor, maxRotor);
        if (rotorInput != null)
            rotorInput.text = clampedDiameter.ToString("0.00");

        ApplyScale(clampedDiameter);
    }

    // ✅ Public button hooks
    public void IncreaseRotor()
    {
        float current = GetCurrentRotorDiameter();
        UpdateScaleFromDiameter(current + step);
    }

    public void DecreaseRotor()
    {
        float current = GetCurrentRotorDiameter();
        UpdateScaleFromDiameter(current - step);
    }

    // ---- helpers ----
    float GetCurrentRotorDiameter()
    {
        if (rotorInput != null && float.TryParse(rotorInput.text, out float v))
            return v;

        // Fallback to base derived from current scale
        return Mathf.Clamp(realScale * baseDiameter, minRotor, maxRotor);
    }

    void ApplyScale(float clampedDiameter)
    {
        realScale = clampedDiameter / baseDiameter;

        if (targetObject != null)
        {
            float visualScale = realScale * displayToRealScaleRatio;
            targetObject.transform.localScale = Vector3.one * visualScale;
        }

        if (scaleText != null)
            scaleText.text = $"Scale: {realScale:0.000}";

        if (rotorSizeText != null)
            rotorSizeText.text = $"Rotor Size: {clampedDiameter:0.00} m";
    }
}
