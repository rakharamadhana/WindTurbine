using UnityEngine;
using UnityEngine.UI; // for Image
using System.Collections;

public enum SlotType
{
    WindTurbine,
    Transformer,
    Substation,
    Pole,
    Building
}

public class PlacementSlot : MonoBehaviour
{
    public SlotType slotCategory;

    public bool isOccupied = false;
    private GameObject currentPlacedObject;
    public GameObject placedObject;

    [Header("Feedback Visuals")]
    public SpriteRenderer hoverSquareRenderer;
    public Color defaultColor = new Color(1, 1, 1, 0.2f);
    public Color validColor = new Color(0, 1, 0, 0.4f);   // Green transparent
    public Color invalidColor = new Color(1, 0, 0, 0.4f); // Red transparent

    public GameObject hoverTextObject; // Assign in Inspector


    public bool CanPlace(GameObject objectToPlace)
    {
        PlaceableObject placeable = objectToPlace.GetComponent<PlaceableObject>();
        return placeable != null && placeable.objectCategory == slotCategory;
    }

    public void ShowHoverFeedback(GameObject objectToPlace)
    {
        if (hoverSquareRenderer == null)
        {
            Debug.LogWarning("No hoverSquareRenderer assigned.");
            return;
        }

        // ✅ Skip feedback if slot is already occupied
        if (isOccupied)
        {
            hoverSquareRenderer.enabled = false;
            return;
        }

        hoverSquareRenderer.enabled = true;

        if (CanPlace(objectToPlace))
        {
            hoverSquareRenderer.color = validColor;
        }
        else
        {
            hoverSquareRenderer.color = invalidColor;
        }
    }

    IEnumerator FadeOutHover(SpriteRenderer renderer)
    {
        float duration = 0.3f;
        float t = 0;
        Color initial = renderer.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.color = Color.Lerp(initial, new Color(initial.r, initial.g, initial.b, 0), t / duration);
            yield return null;
        }
        renderer.enabled = false;
    }


    public void ClearHoverFeedback()
    {
        if (hoverSquareRenderer != null)
        {
            hoverSquareRenderer.color = defaultColor;
            if (isOccupied)
            {
                // Hide the hover square entirely if already 
                hoverSquareRenderer.enabled = false;
            }
            else
            {
                // Reset to default color but keep visible for empty slot
                hoverSquareRenderer.enabled = true;
            }
        }
    }

    public void Place(GameObject newObject)
    {
        if (!CanPlace(newObject))
        {
            Debug.LogWarning("❌ Cannot place object in this slot.");
            return;
        }

        if (placedObject != null)
            Destroy(placedObject);

        PlaceableObject placeable = newObject.GetComponent<PlaceableObject>();

        newObject.transform.SetParent(transform, false);
        newObject.transform.localPosition = Vector3.zero;

        if (placeable.retainTransform)
        {
            Quaternion originalRotation = newObject.transform.rotation;
            Vector3 originalScale = newObject.transform.lossyScale;

            newObject.transform.SetParent(transform, false);
            newObject.transform.localPosition = Vector3.zero;

            newObject.transform.rotation = originalRotation;
            newObject.transform.localScale = originalScale;
        }
        else
        {
            newObject.transform.localRotation = Quaternion.identity;
            newObject.transform.localScale = Vector3.one;
        }

        placedObject = newObject;
        isOccupied = true;

        if (hoverTextObject != null)
            hoverTextObject.SetActive(false);

        ClearHoverFeedback();
    }

    public void Unplace()
    {
        if (placedObject)
        {
            Destroy(placedObject);
            placedObject = null;
        }
        isOccupied = false;

        // Show placeholder again
        if (hoverTextObject) hoverTextObject.SetActive(true);

        // Restore empty hover visuals
        if (hoverSquareRenderer)
        {
            hoverSquareRenderer.enabled = true;
            hoverSquareRenderer.color = defaultColor;
        }
    }

    public void ClearSlot()
    {
        if (currentPlacedObject != null)
        {
            Destroy(currentPlacedObject);
            currentPlacedObject = null;
        }
        isOccupied = false;
    }
}


