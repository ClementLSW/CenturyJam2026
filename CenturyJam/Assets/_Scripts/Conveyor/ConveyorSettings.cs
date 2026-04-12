using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ConveyorSettings", menuName = "Game/ConveyorSettings")]
public class ConveyorSettings : ScriptableObject
{
    [Header("Slot Config")]
    public int slotCount = 3;
    public float slotSpacing = 1.5f;

    [Header("Parcel Pool")]
    public List<ParcelPoolEntry> parcelPool;

    [Header("Difficulty Scaling")]
    [Range(0f, 1f)] public float startingComplexityBias = 0.2f;
    [Range(0f, 1f)] public float maxComplexityBias = 0.8f;
    [Range(1, 100)] public int windowSize = 10;
}

[System.Serializable]
public class ParcelPoolEntry
{
    public ParcelData parcel;
    [Range(1, 5)] public int complexity;
    [Range(0f, 1f)] public float baseWeight = 1f;
}
