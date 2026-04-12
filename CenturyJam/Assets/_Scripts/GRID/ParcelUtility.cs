using UnityEngine;
using System.Collections.Generic;

public static class ParcelUtility
{
    /// <summary>
    /// Rotates shape offsets 90 degrees clockwise per step.
    /// (x, y) -> (y, -x) per rotation.
    /// </summary>
    public static List<Vector2Int> RotateShape(List<Vector2Int> offsets, int steps)
    {
        steps = ((steps % 4) + 4) % 4; // normalize to 0-3
        var result = new List<Vector2Int>(offsets);
        for (int i = 0; i < steps; i++)
            for (int j = 0; j < result.Count; j++)
                result[j] = new Vector2Int(result[j].y, -result[j].x);
        return result;
    }
}