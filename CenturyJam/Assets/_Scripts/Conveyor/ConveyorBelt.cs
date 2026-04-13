using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ConveyorSettings settings;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;

    private WorldParcel currentParcel;
    private int ownerID;
    public int OwnerID => ownerID;
    private int totalSpawned = 0;
    private Color playerColor;

    public void Initialize(int playerID, Color color)
    {
        ownerID = playerID;
        playerColor = color;
    }

    public void SpawnParcel()
    {
        if (currentParcel != null) return;

        ParcelData data = PickParcel();
        totalSpawned++;

        GameObject go = new GameObject($"Parcel_{ownerID}");
        go.transform.position = waypoints[0].position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = data.parcelSprite;
        sr.color = playerColor;
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var parcel = go.AddComponent<WorldParcel>();
        parcel.Initialize(this, data, waypoints, moveSpeed);

        currentParcel = parcel;
    }

    public WorldParcel TryTakeParcel()
    {
        if (currentParcel == null) return null;
        if (!currentParcel.IsInteractable()) return null;

        var p = currentParcel;
        currentParcel = null;

        return p;
    }

    public bool HasParcelReady()
    {
        return currentParcel != null && currentParcel.IsInteractable();
    }

    private ParcelData PickParcel()
    {
        float t = Mathf.Clamp01((float)totalSpawned / settings.windowSize);
        float complexityBias = Mathf.Lerp(
            settings.startingComplexityBias,
            settings.maxComplexityBias, t);

        float totalWeight = 0f;
        float[] weights = new float[settings.parcelPool.Count];

        for (int i = 0; i < settings.parcelPool.Count; i++)
        {
            var entry = settings.parcelPool[i];
            float complexityFactor = Mathf.Lerp(1f, entry.complexity / 5f, complexityBias);
            float weight = entry.baseWeight * complexityFactor;

            weights[i] = weight;
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

    public WorldParcel GetReadyParcel()
    {
        if (currentParcel != null && currentParcel.IsInteractable())
            return currentParcel;

        return null;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] && waypoints[i + 1])
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}