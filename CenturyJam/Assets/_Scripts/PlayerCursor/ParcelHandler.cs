using UnityEngine;

public class ParcelHandler : MonoBehaviour
{
    [SerializeField] private ConveyorManager conveyorManager;
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
        conveyorManager = FindObjectOfType<ConveyorManager>();
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
        // Check own belt first
        ConveyorBelt myBelt = conveyorManager.GetBelt(cursor.PlayerIndex);
        if (myBelt != null)
        {
            int slot = myBelt.GetSlotAtPosition(transform.position, pickupRange);
            if (slot != -1)
            {
                WorldParcel wp = myBelt.GrabFromSlot(slot);
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
                CellData cell = gridManager.GetCell(gridPos);
                ParcelData data = gridManager.GetParcelDataAt(gridPos);
                int originalOwner = cell.ownerID;

                gridManager.RemoveParcel(parcelId);

                var go = new GameObject("PickedParcel");
                go.transform.position = transform.position;
                var wp = go.AddComponent<WorldParcel>();
                wp.data = data;
                wp.ownerID = originalOwner;
                wp.parcelID = parcelId;
                wp.sourceSlotIndex = -1;
                wp.sourceBelt = null;

                PickUp(wp);
                return;
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
        heldVisual.color = cursor.PlayerColor;
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
                    currentRotation, cursor.PlayerIndex, heldParcel.data);
                heldParcel.parcelID = id;
                heldParcel.ownerID = cursor.PlayerIndex;

                if (heldParcel.sourceBelt != null && heldParcel.sourceSlotIndex >= 0)
                    {
                        heldParcel.sourceBelt.OnParcelCommitted(heldParcel.sourceSlotIndex);
                    }

                CleanupHeld();
                return;
            }
        }

       if (heldParcel.ownerID == cursor.PlayerIndex)
        {
            ConveyorBelt myBelt = conveyorManager.GetBelt(cursor.PlayerIndex);
            if (myBelt != null && myBelt.TryReturnParcel(heldParcel, transform.position))
            {
                heldParcel.parcelID = -1;
                if (heldVisual != null) Destroy(heldVisual.gameObject);
                heldParcel = null;
                heldVisual = null;
                currentRotation = 0;
                return;
            }
        }

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
        if (heldParcel != null) Destroy(heldParcel.gameObject);
        heldParcel = null;
        heldVisual = null;
        currentRotation = 0;
    }
}