using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMover : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 1.0f;

    private float t = 0;

    void Update()
    {
        t += Time.deltaTime * speed;
        if (t > 1f) t = 0f; // Loop

        transform.position = Vector3.Lerp(pointA.position, pointB.position, t);
    }
}

