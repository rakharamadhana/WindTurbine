// Attach this to your UI Button object and hook OnClick -> OnClickUndo
using UnityEngine;

public class UndoButton : MonoBehaviour
{
    public void OnClickUndo()
    {
        PlacementHistory.UndoLast();
    }
}
