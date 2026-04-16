using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Config")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private TruckTemplateGroup templateGroup;
    [SerializeField] private Sprite cellSprite;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float rippleSpeed = 0.08f;

    // Data layer
    private CellData[,] grid;
    private int width;
    private int height;

    public int Width => width;
    public int Height => height;

    private Dictionary<int, ParcelData> parcelRegistry = new Dictionary<int, ParcelData>();
    private Dictionary<int, Color> parcelColorRegistry = new Dictionary<int, Color>();
    private Dictionary<int, GameObject> placedVisuals = new Dictionary<int, GameObject>();

    private SpriteRenderer[,] cellRenderers;

    // Ghost preview: maps ownerId -> list of grid cells being previewed, and the player's color
    private Dictionary<int, (List<Vector2Int> cells, Color color)> ghostPreviews = new Dictionary<int, (List<Vector2Int>, Color)>();

    private static readonly Color WarningColor = new Color(1f, 0.5f, 0f); // orange

    private Vector2 Origin => (Vector2)transform.position;

    private int nextparcelId = 1;


    public void LoadRandomTemplate()
    {
        if (templateGroup == null)
        {
            Debug.LogError("Template group not assigned!");
            return;
        }

        TruckTemplate t = templateGroup.GetRandomTemplate();
        if (t != null)
        {
            LoadTemplate(t);
        }
    }
    public void LoadTemplate(TruckTemplate t)
    {
        width = t.width;
        height = t.height;
        grid = new CellData[width, height];

        foreach (var blocked in t.blockedCells)
        {
            if (InBounds(blocked))
                grid[blocked.x, blocked.y].state = CellState.Blocked;
        }

        SpawnVisuals();

        // Ripple IN
        SetGridAlpha(0f);
        StartCoroutine(RippleFade(0f, 1f));
    }

    //public void FadeOutAndReload(TruckTemplate t)
    //{
    //    StartCoroutine(FadeOutThenReload(t));
    //}

    //private IEnumerator FadeOutThenReload(TruckTemplate t)
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    yield return StartCoroutine(RippleFade(1f, 0f));
    //    LoadTemplate(t);
    //}


    public void FadeOutAndReloadRandom()
    {
        StartCoroutine(FadeOutThenReloadRandom());
    }

    private IEnumerator FadeOutThenReloadRandom()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(RippleFade(1f, 0f));

        LoadRandomTemplate();
    }

    private IEnumerator RippleFade(float startAlpha, float endAlpha)
    {
        float time = 0f;

        // Ripple origin (center of grid)
        Vector2 center = new Vector2(width / 2f, height / 2f);

        while (time < fadeDuration)
        {
            float globalT = time / fadeDuration;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var sr = cellRenderers[x, y];
                    if (sr == null) continue;

                    float dist = Vector2.Distance(new Vector2(x, y), center);

                    float delay = dist * rippleSpeed;

                    float localT = Mathf.Clamp01(globalT - delay);

                    float alpha = Mathf.Lerp(startAlpha, endAlpha, localT);

                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        SetGridAlpha(endAlpha);
    }


    private void SetGridAlpha(float alpha)
    {
        if (cellRenderers == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var sr = cellRenderers[x, y];
                if (sr == null) continue;

                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    public bool IsInsideBounds(Vector2 worldPos)
    {
        Vector2 local = worldPos - Origin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 local = worldPos - Origin;
        return new Vector2Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.y / cellSize)
        );
    }

    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        return Origin + new Vector2(
            gridPos.x * cellSize + cellSize * 0.5f,
            gridPos.y * cellSize + cellSize * 0.5f
        );
    }

    public bool CanPlace(List<Vector2Int> shapeOffsets, Vector2Int gridPos, int rotation)
    {
        var rotated = ParcelUtility.RotateShape(shapeOffsets, rotation);

        foreach (var offset in rotated)
        {
            Vector2Int cell = gridPos + offset;

            if (!InBounds(cell)) return false;
            if (grid[cell.x, cell.y].state != CellState.Empty) return false;
        }

        return true;
    }

    public int PlaceParcel(List<Vector2Int> shapeOffsets, Vector2Int gridPos,
                          int rotation, int ownerID, ParcelData data, Color parcelColor)
    {
        var rotated = ParcelUtility.RotateShape(shapeOffsets, rotation);
        int parcelId = nextparcelId++;

        foreach (var offset in rotated)
        {
            Vector2Int cell = gridPos + offset;

            grid[cell.x, cell.y].state = CellState.Occupied;
            grid[cell.x, cell.y].ownerID = ownerID;
            grid[cell.x, cell.y].parcelID = parcelId;
        }

        parcelRegistry[parcelId] = data;
        parcelColorRegistry[parcelId] = parcelColor;
        RefreshVisuals();

        return parcelId;
    }

    public void RemoveParcel(int parcelId)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].parcelID == parcelId)
                    grid[x, y] = new CellData();
            }
        }

        if (placedVisuals.TryGetValue(parcelId, out var visual))
        {
            Destroy(visual);
            placedVisuals.Remove(parcelId);
        }

        parcelRegistry.Remove(parcelId);
        parcelColorRegistry.Remove(parcelId);
        RefreshVisuals();
    }

    public void RegisterPlacedVisual(int parcelId, GameObject visual)
    {
        placedVisuals[parcelId] = visual;
    }


    public CellData GetCell(Vector2Int gridPos)
    {
        if (!InBounds(gridPos))
            return new CellData { state = CellState.Blocked };

        return grid[gridPos.x, gridPos.y];
    }

    public int GetParcelIdAt(Vector2Int gridPos)
    {
        if (!InBounds(gridPos)) return -1;
        if (grid[gridPos.x, gridPos.y].state != CellState.Occupied) return -1;

        return grid[gridPos.x, gridPos.y].parcelID;
    }

    public ParcelData GetParcelDataAt(Vector2Int gridPos)
    {
        int id = GetParcelIdAt(gridPos);
        if (id == -1) return null;

        return parcelRegistry.ContainsKey(id) ? parcelRegistry[id] : null;
    }

    public Color GetParcelColorAt(Vector2Int gridPos)
    {
        int id = GetParcelIdAt(gridPos);
        if (id == -1) return Color.white;

        return parcelColorRegistry.ContainsKey(id) ? parcelColorRegistry[id] : Color.white;
    }

    private bool InBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width &&
               pos.y >= 0 && pos.y < height;
    }


    private void SpawnVisuals()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        cellRenderers = new SpriteRenderer[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var go = new GameObject($"Cell_{x}_{y}");
                go.transform.parent = transform;
                go.transform.position = GridToWorld(new Vector2Int(x, y));
                go.transform.localScale = Vector3.one * cellSize * 0.95f;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = cellSprite;

                cellRenderers[x, y] = sr;
            }
        }

        RefreshVisuals();
    }

    public void SetGhostPreview(int ownerId, List<Vector2Int> cells, Color playerColor)
    {
        ghostPreviews[ownerId] = (cells, playerColor);
        RefreshVisuals();
    }

    public void ClearGhostPreview(int ownerId)
    {
        if (ghostPreviews.Remove(ownerId))
            RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        // Build a map of ghost occupancy per cell: ownerId -> color
        // Used to detect overlap between players' ghosts
        var ghostMap = new Dictionary<Vector2Int, List<(int ownerId, Color color)>>();
        foreach (var kv in ghostPreviews)
        {
            foreach (var cell in kv.Value.cells)
            {
                if (!ghostMap.ContainsKey(cell))
                    ghostMap[cell] = new List<(int, Color)>();
                ghostMap[cell].Add((kv.Key, kv.Value.color));
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = grid[x, y];
                var pos = new Vector2Int(x, y);

                Color baseColor = cell.state switch
                {
                    CellState.Empty => new Color(0.9f, 0.9f, 0.9f),
                    CellState.Blocked => new Color(0.3f, 0.3f, 0.3f),
                    CellState.Occupied => new Color(0.9f, 0.9f, 0.9f),
                    _ => Color.white
                };

                if (ghostMap.TryGetValue(pos, out var owners))
                {
                    bool conflict = owners.Count > 1 || cell.state == CellState.Occupied || cell.state == CellState.Blocked;
                    baseColor = conflict ? WarningColor : owners[0].color;
                }

                // preserve alpha for ripple
                baseColor.a = cellRenderers[x, y].color.a;

                cellRenderers[x, y].color = baseColor;
            }
        }
    }
}