using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewParcel", menuName = "Game/Parcel Data")]
public class ParcelData : ScriptableObject
{
    public string parcelName;
    public List<Vector2Int> shapeOffsets;
    public int pointValue;
    public Sprite parcelSprite; // artist-drawn sprite
}