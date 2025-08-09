using UnityEngine;

public class ArrowAlongWire : MonoBehaviour
{
    public LineRenderer wireLine;
    public float speed = 1f;

    public float startOffset = 0f;
    private float t = 0f;

    void Start()
    {
        t = startOffset; // start from an offset so arrows are staggered
    }


    void Update()
    {
        if (wireLine == null)
        {
            Debug.LogWarning("⚠️ Missing wireLine on " + gameObject.name);
            return;
        }

        if (wireLine.positionCount < 2)
            return;

        t += Time.deltaTime * speed;
        if (t > 1f) t = 0f;

        Vector3 start = wireLine.GetPosition(0);
        Vector3 end = wireLine.GetPosition(1);
        Vector3 pos = Vector3.Lerp(start, end, t);

        transform.position = pos;

        Vector3 dir = (end - start).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
