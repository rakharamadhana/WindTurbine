#if UNITY_WEBGL && !UNITY_EDITOR
using UnityEngine;
#endif

using UnityEngine;

public class WebGLKeyboardEnabler : MonoBehaviour
{
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false; // let browser inputs receive focus
#endif
    }
}
