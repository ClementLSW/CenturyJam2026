using UnityEngine;

public class GhostRenderer : MonoBehaviour
{
    [SerializeField] private ParcelVisual ghostParcelPrefab;
    [SerializeField] private GridManager gridManager;

    private ParcelHandler handler;
    private PlayerCursor cursor;
    private ParcelVisual ghostVisual;
    private bool ghostActive;
    private Vector2Int lastSnappedCell;
    private Animator _handSpriteAnimator;

    void Start()
    {
        _handSpriteAnimator = GetComponent<Animator>();
        handler = GetComponent<ParcelHandler>();
        cursor = GetComponent<PlayerCursor>();

        ghostVisual = Instantiate(ghostParcelPrefab);
        ghostVisual.gameObject.SetActive(false);
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

            if (!ghostActive)
            {
                ghostVisual.gameObject.SetActive(true);
                ghostActive = true;
                lastSnappedCell = new Vector2Int(-999, -999);
            }

            if (snapped != lastSnappedCell)
            {
                lastSnappedCell = snapped;
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
        ghostVisual.transform.position = gridManager.GridToWorld(snapped);
        ghostVisual.Build(handler.HeldData, handler.CurrentRotation, cursor.PlayerColor);

        bool valid = gridManager.CanPlace(
            handler.HeldData.shapeOffsets, snapped, handler.CurrentRotation);
        ghostVisual.SetColor(valid
            ? new Color(0, 1, 0, 0.4f)
            : new Color(1, 0, 0, 0.4f));
    }

    private void HideGhost()
    {
        if (ghostActive)
        {
            ghostVisual.gameObject.SetActive(false);
            ghostActive = false;
        }
    }
}