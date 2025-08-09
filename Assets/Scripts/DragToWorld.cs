using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragToWorld : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabToSpawn;
    private GameObject dragPreview;
    private Camera mainCam;
    private Vector3 velocity = Vector3.zero; // For SmoothDamp if you want

    void Start()
    {
        mainCam = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragPreview = Instantiate(prefabToSpawn);

        // Set to Preview layer to exclude from raycast
        dragPreview.layer = LayerMask.NameToLayer("Preview");

        // Disable its collider for safety (optional)
        Collider2D col = dragPreview.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        SpriteRenderer[] renderers = dragPreview.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            r.color = new Color(r.color.r, r.color.g, r.color.b, 0.5f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragPreview)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 targetPos = mainCam.ScreenToWorldPoint(mousePos);
            targetPos.z = 0;

            float smoothSpeed = 10f * Time.deltaTime;
            dragPreview.transform.position = Vector3.Lerp(dragPreview.transform.position, targetPos, smoothSpeed);

            // Track hit slots
            PlacementSlot[] allSlots = FindObjectsOfType<PlacementSlot>();
            PlacementSlot hoveredSlot = null;

            Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);
            foreach (var hit in hits)
            {
                PlacementSlot slot = hit.GetComponent<PlacementSlot>();
                if (slot != null)
                {
                    hoveredSlot = slot;
                    slot.ShowHoverFeedback(dragPreview);
                }
            }

            // ✅ Clear others not being hovered
            foreach (var slot in allSlots)
            {
                if (slot != hoveredSlot)
                {
                    slot.ClearHoverFeedback();
                }
            }

            // Apply preview color based on placement validity
            Color feedbackColor = (hoveredSlot != null && hoveredSlot.CanPlace(dragPreview))
                ? new Color(0.5f, 1f, 0.5f, 0.6f)
                : new Color(1f, 0.4f, 0.4f, 0.4f);

            SpriteRenderer[] renderers = dragPreview.GetComponentsInChildren<SpriteRenderer>();
            foreach (var r in renderers)
            {
                r.color = feedbackColor;
            }
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragPreview)
        {
            Vector3 dropPos = dragPreview.transform.position;

            // Disable colliders of placed objects
            PlacementSlot[] allSlots = FindObjectsOfType<PlacementSlot>();
            List<Collider2D> disabledColliders = new List<Collider2D>();
            foreach (var slot in allSlots)
            {
                if (slot.placedObject != null)
                {
                    Collider2D col = slot.placedObject.GetComponent<Collider2D>();
                    if (col != null && col.enabled)
                    {
                        col.enabled = false;
                        disabledColliders.Add(col);
                    }
                }
            }

            // ✅ Ignore preview layer during hit test
            int layerMask = ~LayerMask.GetMask("IgnoreDrop");
            Collider2D hit = Physics2D.OverlapPoint(dropPos, layerMask);

            foreach (var col in disabledColliders) col.enabled = true;

            if (hit != null)
            {
                PlacementSlot slot = hit.GetComponent<PlacementSlot>();

                if (slot != null && !slot.isOccupied)
                    slot.ClearHoverFeedback();

                if (slot != null && slot.CanPlace(dragPreview))
                {
                    slot.Place(dragPreview);

                    SpriteRenderer[] renderers = dragPreview.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var r in renderers) r.color = Color.white;

                    Collider2D dragCol = dragPreview.GetComponent<Collider2D>();
                    if (dragCol != null) dragCol.enabled = true;

                    dragPreview = null;
                    return;
                }
            }
            Destroy(dragPreview);
        }
    }
}
