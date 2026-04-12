using UnityEngine;

public class ParcelHandler : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private float pickupRange = 1f;

    private PlayerCursor cursor;
    private GhostRenderer ghostRenderer;
    private WorldParcel heldParcel;
    private SpriteRenderer heldVisual;
    private int currentRotation;

    void Start()
    {
        cursor = GetComponent<PlayerCursor>();
        ghostRenderer = GetComponent<GhostRenderer>();
    }

    public bool IsHolding => heldParcel != null;
    public ParcelData HeldData => heldParcel?.data;
    public int CurrentRotation => currentRotation;

    public void HandleInteract()
    {
        if (heldParcel == null)
            TryPickUp();
        else
            TryPlace();
    }

    public void HandleRotateCW()
    {
        if (heldParcel == null) return;
        currentRotation = (currentRotation + 1) % 4;
        ghostRenderer.ForceRefresh();
    }

    public void HandleRotateCCW()
    {
        if (heldParcel == null) return;
        currentRotation = (currentRotation + 3) % 4;
        ghostRenderer.ForceRefresh();
    }

    private void TryPickUp()
    {
        // Uncommitted parcels — own only
        var hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);
        foreach (var hit in hits)
        {
            var wp = hit.GetComponent<WorldParcel>();
            if (wp != null && wp.ownerID == cursor.PlayerIndex && wp.parcelID == -1)
            {
                PickUp(wp);
                return;
            }
        }

        // Committed parcels on grid — anyone's
        if (gridManager.IsInsideBounds(transform.position))
        {
            Vector2Int gridPos = gridManager.WorldToGrid(transform.position);
            int parcelId = gridManager.GetParcelIdAt(gridPos);
            if (parcelId != -1)
            {
                gridManager.RemoveParcel(parcelId);
                // TODO: reconstruct WorldParcel from parcel registry
            }
        }
    }

    private void PickUp(WorldParcel wp)
    {
        heldParcel = wp;
        heldParcel.gameObject.SetActive(false);
        currentRotation = 0;

        var go = new GameObject("HeldVisual");
        heldVisual = go.AddComponent<SpriteRenderer>();
        heldVisual.sprite = wp.data.parcelSprite;
        heldVisual.sortingOrder = 10;
    }

    private void TryPlace()
    {
        // Can only place on valid grid spot
        if (gridManager.IsInsideBounds(transform.position))
        {
            Vector2Int gridPos = gridManager.WorldToGrid(transform.position);
            if (gridManager.CanPlace(heldParcel.data.shapeOffsets, gridPos, currentRotation))
            {
                int id = gridManager.PlaceParcel(
                    heldParcel.data.shapeOffsets, gridPos,
                    currentRotation, cursor.PlayerIndex);
                heldParcel.parcelID = id;
                heldParcel.ownerID = cursor.PlayerIndex;
                CleanupHeld();
                return;
            }
        }

        // TODO: check if cursor is over own belt — return parcel

        Debug.Log("Can't place here");
    }

    void Update()
    {
        if (heldVisual != null)
            heldVisual.transform.position = transform.position;
    }

    public void CleanupHeld()
    {
        if (heldVisual != null) Destroy(heldVisual.gameObject);
        heldParcel = null;
        heldVisual = null;
        currentRotation = 0;
    }
}