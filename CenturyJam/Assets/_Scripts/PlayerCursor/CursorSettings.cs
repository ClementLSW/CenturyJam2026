using UnityEngine;

[CreateAssetMenu(fileName = "CursorSettings", menuName = "Scriptable Objects/CursorSettings")]
public class CursorSettings : ScriptableObject
{
    [Range(1f, 10f)] public float MoveSpeed;
    public Color CursorColor;
}
