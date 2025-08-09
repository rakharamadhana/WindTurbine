using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireFlow : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Material mat;

    void Start()
    {
        mat = GetComponent<LineRenderer>().material;
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        mat.mainTextureOffset = new Vector2(offset, 0);  // Scroll in X direction
    }
}