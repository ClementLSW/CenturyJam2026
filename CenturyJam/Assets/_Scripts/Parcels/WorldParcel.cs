using UnityEngine;

public class WorldParcel : MonoBehaviour
{
    [SerializeField] public ParcelData data;
    public int ownerID = -1; // -1 = unowned/on belt, 0+ = player who placed it
    public int parcelID = -1; // -1 = not on grid, 0+ = placed on grid
    public int sourceSlotIndex = -1;
    public ConveyorBelt sourceBelt;

    public void Setup(ParcelData parcelData, int owner)
    {
        data = parcelData;
        ownerID = owner;
        GetComponent<SpriteRenderer>().sprite = parcelData.parcelSprite;
    }
}