using UnityEngine;
using TMPro;
using NotSlot.HandPainted2D;
using System.Runtime.InteropServices;

public class WindUIController : MonoBehaviour
{
    [Header("References")]
    public GlobalWindController windController;
    public WindZone windZone;
    public TMP_InputField windInput;       // ✅ Numeric input only
    public TextMeshProUGUI windSpeedText;

    [Header("Wind Settings")]
    public float maxWindSpeed = 15f;       // Max wind speed in m/s
    public float initialWind = 1f;         // ✅ Default starting wind

    public static float CurrentWindSpeed { get; private set; } = 1f; // Default fallback

    public static float MaxWindSpeed => Instance?.maxWindSpeed ?? 15f;
    public static WindUIController Instance;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WebGLFocusNumeric(string goName, string initial, float min, float max, float step);

    [DllImport("__Internal")]
    private static extern void WebGLBlurNumeric();
#endif

    // Call this when the TMP input is selected (or from a small "edit" button)
    public void OpenMobileKeyboard()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // IMPORTANT: on iOS Safari, this must be called from a real user tap (OnPointerDown/OnSelect)
        WebGLFocusNumeric(gameObject.name, windInput != null ? windInput.text : "0", 0f, maxWindSpeed, 0.1f);
#endif
        // Also activate TMP field so caret & selection match
        if (windInput != null) windInput.ActivateInputField();
    }

    public void CloseMobileKeyboard()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLBlurNumeric();
#endif
        if (windInput != null) windInput.DeactivateInputField();
    }

    // Called from JS when user types
    public void OnHtmlInputChanged(string val)
    {
        if (windInput != null) windInput.text = val;
        UpdateWindFromInput(val);
    }

    // Optional: handle Enter/Done
    public void OnHtmlInputSubmit(string val)
    {
        OnHtmlInputChanged(val);
        CloseMobileKeyboard();
    }

    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        if (windInput != null)
        {
            // ✅ Set initial value
            float startValue = Mathf.Clamp(initialWind, 0f, maxWindSpeed);
            windInput.text = startValue.ToString("0.00");

            // ✅ Update live while typing
            windInput.onValueChanged.AddListener(UpdateWindFromInput);
            UpdateWindFromInput(windInput.text); // Initialize with starting wind
        }
    }

    void UpdateWindFromInput(string input)
    {
        if (!float.TryParse(input, out float windInMetersPerSecond))
            return;

        // ✅ Clamp input immediately
        float clampedWind = Mathf.Clamp(windInMetersPerSecond, 0f, maxWindSpeed);

        // ✅ Overwrite input if out of range
        if (Mathf.Abs(clampedWind - windInMetersPerSecond) > 0.001f)
            windInput.text = clampedWind.ToString("0.00");

        // ✅ Apply to WindZone
        if (windZone != null)
            windZone.windMain = clampedWind;

        // ✅ Apply to global wind controller
        if (windController != null)
            windController.SetGlobalWindSpeed(clampedWind);

        // ✅ Update UI text
        if (windSpeedText != null)
            windSpeedText.text = $"Wind Speed: {clampedWind:0.00} m/s";

        AudioManager.Instance.PlayWindLoop();
        AudioManager.Instance.UpdateWindVolume(clampedWind * 1.2f, maxWindSpeed);

        CurrentWindSpeed = clampedWind;
    }

    public void IncreaseWind()
    {
        ChangeWindBy(1f); // step size — adjust as needed
    }

    public void DecreaseWind()
    {
        ChangeWindBy(-1f);
    }

    private void ChangeWindBy(float amount)
    {
        float currentValue = CurrentWindSpeed;
        float newValue = Mathf.Clamp(currentValue + amount, 0f, maxWindSpeed);

        windInput.text = newValue.ToString("0.00");
        // This will trigger UpdateWindFromInput automatically
    }

}
