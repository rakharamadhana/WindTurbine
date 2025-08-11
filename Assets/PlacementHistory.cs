using System.Collections.Generic;
using UnityEngine;

public static class PlacementHistory
{
    public struct Record
    {
        public PlacementSlot slot;
        public GameObject obj;
    }

    private static readonly Stack<Record> _history = new();

    public static void Register(PlacementSlot slot, GameObject obj)
    {
        if (slot != null && obj != null)
            _history.Push(new Record { slot = slot, obj = obj });
    }

    public static bool CanUndo => _history.Count > 0;

    public static void UndoLast()
    {
        while (_history.Count > 0)
        {
            var rec = _history.Pop();
            if (rec.slot == null) continue; // slot might have been destroyed

            // If the object still exists, let the slot handle destroying it
            // and restoring placeholder/hover visuals.
            if (rec.obj != null)
            {
                rec.slot.Unplace();
            }
            else
            {
                // Object already gone => just restore slot visuals/state
                rec.slot.Unplace();
            }
            break;
        }
    }


    public static void Clear()
    {
        _history.Clear();
    }
}
