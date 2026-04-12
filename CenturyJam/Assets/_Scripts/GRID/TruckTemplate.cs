using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewTruck", menuName = "Game/Truck Template")]
public class TruckTemplate : ScriptableObject
{
    public int width = 8;
    public int height = 6;
    public List<Vector2Int> blockedCells = new List<Vector2Int>();
}