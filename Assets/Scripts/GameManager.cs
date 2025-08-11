using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlayBGM();
        AudioManager.Instance.PlaySeaBGM();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetScene()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);   // works on WebGL, Android, etc.
        // (Make sure the scene is in Build Settings)
    }

    // Call Register when you spawn/place an item
    private static readonly List<GameObject> placed = new();

    public static void Register(GameObject go)
    {
        if (go != null && !placed.Contains(go)) placed.Add(go);
    }

    // Hook this to your Reset/Ulangi button
    public void ClearAllPlaced()
    {
        for (int i = placed.Count - 1; i >= 0; i--)
        {
            if (placed[i] != null) Destroy(placed[i]);
            placed.RemoveAt(i);
        }

        // Optional: reset UI/camera/wind/etc.
        // Camera.main.transform.position = initialCamPos;
        // Camera.main.orthographicSize = initialOrthoSize;
    }
}
