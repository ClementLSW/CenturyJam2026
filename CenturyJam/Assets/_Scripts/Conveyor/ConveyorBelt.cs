using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private ConveyorSettings settings;
    [SerializeField] private Sprite slotSprite;

    private WorldParcel[] slots;
    private Transform[] slotTransforms;
    private int ownerID;
    private int totalSpawned = 0;

public void Initialize(int playerID, Color playerColor)
{
    ownerID = playerID;
    slots = new WorldParcel[settings.slotCount];
    slotTransforms = new Transform[settings.slotCount];

    float totalHeight = (settings.slotCount - 1) * settings.slotSpacing;
    float startY = -totalHeight / 2f;

    for (int i = 0; i < settings.slotCount; i++)
    {
        var slotGO = new GameObject($"Slot_{i}");
        slotGO.transform.parent = transform;
        slotGO.transform.localPosition = new Vector3(0, startY + i * settings.slotSpacing, 0);

        var sr = slotGO.AddComponent<SpriteRenderer>();
        sr.sprite = slotSprite;
        sr.color = new Color(playerColor.r, playerColor.g, playerColor.b, 0.2f);
        sr.sortingOrder = -1;

        slotTransforms[i] = slotGO.transform;
    }
}

    public void FillAllSlots()
    {
        for (int i = 0; i < settings.slotCount; i++)
        {
            if (slots[i] == null)
                SpawnParcelAtSlot(i);
        }
    }

    // --- Grab / Return ---

    public int GetSlotAtPosition(Vector2 worldPos, float range = 0.8f)
    {
        for (int i = 0; i < settings.slotCount; i++)
        {
            if (slots[i] != null &&
                Vector2.Distance(worldPos, slotTransforms[i].position) < range)
                return i;
        }
        return -1;
    }

    public WorldParcel GrabFromSlot(int slotIndex)
    {
        var parcel = slots[slotIndex];
        slots[slotIndex] = null;
        return parcel;
    }

    public void OnParcelCommitted(int slotIndex)
    {
        slots[slotIndex] = null;
        SpawnParcelAtSlot(slotIndex);
    }

    public bool TryReturnParcel(WorldParcel parcel, Vector2 cursorPos)
    {
        int closest = GetClosestEmptySlot(cursorPos);
        if (closest == -1) return false;

        slots[closest] = parcel;
        parcel.transform.position = slotTransforms[closest].position;
        parcel.gameObject.SetActive(true);
        return true;
    }

    public bool IsOwnedBy(int playerID) => ownerID == playerID;

    // --- Spawn logic ---

    private void SpawnParcelAtSlot(int slotIndex)
    {
        ParcelData data = PickParcel();
        totalSpawned++;

        var go = new GameObject($"Parcel_{ownerID}_{slotIndex}");
        go.transform.position = slotTransforms[slotIndex].position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = data.parcelSprite;
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var wp = go.AddComponent<WorldParcel>();
        wp.data = data;
        wp.ownerID = ownerID;
        wp.sourceSlotIndex = slotIndex;
        wp.sourceBelt = this;

        slots[slotIndex] = wp;
    }

    private ParcelData PickParcel()
    {
        float t = Mathf.Clamp01((float)totalSpawned / settings.windowSize);
        float complexityBias = Mathf.Lerp(
            settings.startingComplexityBias,
            settings.maxComplexityBias, t);

        float totalWeight = 0f;
        List<float> weights = new List<float>();

        foreach (var entry in settings.parcelPool)
        {
            float complexityFactor = Mathf.Lerp(1f, entry.complexity / 5f, complexityBias);
            float weight = entry.baseWeight * complexityFactor;
            weights.Add(weight);
            totalWeight += weight;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < settings.parcelPool.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
                return settings.parcelPool[i].parcel;
        }

        return settings.parcelPool[^1].parcel;
    }

    private int GetClosestEmptySlot(Vector2 worldPos)
    {
        int closest = -1;
        float closestDist = float.MaxValue;
        for (int i = 0; i < settings.slotCount; i++)
        {
            if (slots[i] == null)
            {
                float dist = Vector2.Distance(worldPos, slotTransforms[i].position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = i;
                }
            }
        }
        return closest;
    }
}