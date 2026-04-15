using System.Collections.Generic;
using UnityEngine;

public class GhostRenderer : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private ParcelHandler handler;
    private PlayerCursor cursor;
    private bool ghostActive;
    private Vector2Int lastSnappedCell;
    private int lastRotation;
    private Animator _handSpriteAnimator;

    void Start()
    {
        _handSpriteAnimator = GetComponent<Animator>();
        handler = GetComponent<ParcelHandler>();
        cursor = GetComponent<PlayerCursor>();
    }

    void Update()
    {
        _handSpriteAnimator.SetBool("isHeld", handler.IsHolding);

        if (!handler.IsHolding)
        {
            HideGhost();
            return;
        }

        if (gridManager.IsInsideBounds(transform.position))
        {
            Vector2Int snapped = gridManager.WorldToGrid(transform.position);
            int rotation = handler.CurrentRotation;

            if (!ghostActive || snapped != lastSnappedCell || rotation != lastRotation)
            {
                ghostActive = true;
                lastSnappedCell = snapped;
                lastRotation = rotation;
                Refresh(snapped);
            }
        }
        else
        {
            HideGhost();
        }
    }

    public void ForceRefresh()
    {
        if (!ghostActive) return;
        Vector2Int snapped = gridManager.WorldToGrid(transform.position);
        Refresh(snapped);
    }

    private void Refresh(Vector2Int snapped)
    {
        var rotated = ParcelUtility.RotateShape(handler.HeldData.shapeOffsets, handler.CurrentRotation);
        var cells = new List<Vector2Int>();
        foreach (var offset in rotated)
            cells.Add(snapped + offset);

        Color playerColor = cursor.PlayerColor;
        playerColor.a = 1f; // cells are fully tinted — no transparency needed

        gridManager.SetGhostPreview(cursor.PlayerIndex, cells, playerColor);
    }

    private void HideGhost()
    {
        if (ghostActive)
        {
            gridManager.ClearGhostPreview(cursor.PlayerIndex);
            ghostActive = false;
        }
    }

    public void SetDependency(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }
}
