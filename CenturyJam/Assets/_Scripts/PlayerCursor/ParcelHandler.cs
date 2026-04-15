using UnityEngine;
using System.Collections.Generic;

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

    [SerializeField] private GameObject placeEffect;


    void Start()
    {
        cursor = GetComponent<PlayerCursor>();
        ghostRenderer = GetComponent<GhostRenderer>();
        conveyorManager = FindObjectOfType<ConveyorManager>();
    }

    public bool IsHolding => heldParcel != null;
    public ParcelData HeldData => heldParcel?.data;
    public int CurrentRotation => currentRotation;

    public void SetDependency(ConveyorManager conveyorManager, GridManager gridManager)
    {
        if (this.conveyorManager == null) this.conveyorManager = conveyorManager;
        if (this.gridManager == null) this.gridManager = gridManager;
    }

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
        if (heldVisual != null)
            heldVisual.transform.rotation = Quaternion.Euler(0, 0, -90f * currentRotation);
        ghostRenderer.ForceRefresh();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.boxRotate);
    }

    public void HandleRotateCCW()
    {
        if (heldParcel == null) return;
        currentRotation = (currentRotation + 3) % 4;
        if (heldVisual != null)
            heldVisual.transform.rotation = Quaternion.Euler(0, 0, -90f * currentRotation);
        ghostRenderer.ForceRefresh();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.boxRotate);
    }
    private void TryPickUp()
    {
        List<ConveyorBelt> belts = conveyorManager.GetBelts(cursor.PlayerIndex);

        foreach (var belt in belts)
        {
            WorldParcel wp = belt.GetReadyParcel();

            if (wp == null) continue;

            float dist = Vector2.Distance(
                transform.position,
                wp.transform.position
            );

            if (dist <= pickupRange)
            {
                belt.TryTakeParcel(); // important: clear belt state
                PickUp(wp);
                return;
            }
        }

        if (gridManager.IsInsideBounds(transform.position))
        {
            Vector2Int gridPos = gridManager.WorldToGrid(transform.position);
            int parcelId = gridManager.GetParcelIdAt(gridPos);

            if (parcelId != -1)
            {
                CellData cell = gridManager.GetCell(gridPos);
                ParcelData data = gridManager.GetParcelDataAt(gridPos);
                Color color = gridManager.GetParcelColorAt(gridPos);
                int originalOwner = cell.ownerID;

                gridManager.RemoveParcel(parcelId);

                var go = new GameObject("PickedParcel");
                go.transform.position = transform.position;

                var wp = go.AddComponent<WorldParcel>();
                wp.data = data;
                wp.ownerID = originalOwner;
                wp.parcelID = parcelId;
                wp.parcelColor = color;

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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.boxPickup);

        var go = new GameObject("HeldVisual");
        go.transform.rotation = Quaternion.Euler(0, 0, -90f * currentRotation);
        heldVisual = go.AddComponent<SpriteRenderer>();
        heldVisual.sprite = wp.data.parcelSprite;
        heldVisual.color = wp.parcelColor;
        heldVisual.sortingOrder = 10;
    }


    private void TryPlace()
    {
        // PLACE ON GRID
        if (gridManager.IsInsideBounds(transform.position))
        {
            Vector2Int gridPos = gridManager.WorldToGrid(transform.position);

            if (gridManager.CanPlace(
                heldParcel.data.shapeOffsets,
                gridPos,
                currentRotation))
            {
                int id = gridManager.PlaceParcel(
                    heldParcel.data.shapeOffsets,
                    gridPos,
                    currentRotation,
                    heldParcel.ownerID,
                    heldParcel.data,
                    heldParcel.parcelColor);

                heldParcel.parcelID = id;

                // Spawn placed sprite at center of occupied cells
                var rotated = ParcelUtility.RotateShape(heldParcel.data.shapeOffsets, currentRotation);
                Vector2 sum = Vector2.zero;
                foreach (var offset in rotated)
                    sum += gridManager.GridToWorld(gridPos + offset);
                Vector2 center = sum / rotated.Count;

                var placed = new GameObject($"PlacedParcel_{id}");
                placed.transform.position = center;
                placed.transform.rotation = Quaternion.Euler(0, 0, -90f * currentRotation);
                var placedSr = placed.AddComponent<SpriteRenderer>();
                placedSr.sprite = heldParcel.data.parcelSprite;
                placedSr.color = heldParcel.parcelColor;
                placedSr.sortingOrder = 5;

                gridManager.RegisterPlacedVisual(id, placed);

                conveyorManager.NotifyParcelPlaced(cursor.PlayerIndex); //respawn parcel
                AudioManager.Instance.PlaySFX(AudioManager.Instance.boxDrop);
                AudioManager.Instance.PlaySFXDelayed(AudioManager.Instance.conveyor, 1f);
                if (placeEffect != null) Instantiate (placeEffect, gameObject.transform.position, Quaternion.identity);

                CleanupHeld();
                return;
            }
            else
            {
                Debug.Log("Invalid placement");
                AudioManager.Instance.PlaySFX(AudioManager.Instance.boxInvalid);
                return;
            }
        }

        // ❌ NO MORE RETURN TO BELT
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