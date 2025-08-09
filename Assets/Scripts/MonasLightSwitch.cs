using UnityEngine;

public class MonasLightSwitch : MonoBehaviour
{
    public SpriteRenderer monasRenderer; // Drag your Monas SpriteRenderer
    public Sprite offSprite;             // Default/off sprite
    public Sprite onSprite;              // Lights on sprite

    private bool isOn = false;

    // Example: Call this when power is ON
    public void TurnOnLight()
    {
        isOn = true;
        monasRenderer.sprite = onSprite;
    }

    // Example: Call this when power is OFF
    public void TurnOffLight()
    {
        isOn = false;
        monasRenderer.sprite = offSprite;
    }

    // Optional: Toggle with a key for testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isOn = !isOn;
            monasRenderer.sprite = isOn ? onSprite : offSprite;
        }
    }
}
