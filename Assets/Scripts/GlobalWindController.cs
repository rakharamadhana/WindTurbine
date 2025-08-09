using UnityEngine;

namespace NotSlot.HandPainted2D
{
    public class GlobalWindController : MonoBehaviour
    {
        public void SetGlobalWindSpeed(float newSpeed)
        {
            // Existing logic for WindOptions...
            WindOptionsShape[] shapeObjects = FindObjectsOfType<WindOptionsShape>();
            foreach (var obj in shapeObjects)
            {
                obj.SetSpeed(newSpeed);
            }

            WindOptions[] optionsObjects = FindObjectsOfType<WindOptions>();
            foreach (var obj in optionsObjects)
            {
                obj.SetSpeed(newSpeed);
            }
        }


        public void SetGlobalNoise(float newNoise)
        {
            foreach (var obj in FindObjectsOfType<WindOptionsShape>())
            {
                obj.SetNoise(newNoise);
            }
        }

        public void SetGlobalFlip(bool flip)
        {
            foreach (var obj in FindObjectsOfType<WindOptionsShape>())
            {
                obj.SetFlip(flip);
            }
        }

    }
}
