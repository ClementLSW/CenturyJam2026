using UnityEngine;
using System.Collections.Generic;

public class ParcelVisual : MonoBehaviour
{
    [SerializeField] private Sprite cellSprite;

    private List<SpriteRenderer> cells = new List<SpriteRenderer>();

    public void Build(ParcelData data, int rotation, Color color)
    {
        Clear();

        var offsets = ParcelUtility.RotateShape(data.shapeOffsets, rotation);
        foreach (var offset in offsets)
        {
            var go = new GameObject("ParcelCell");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            go.transform.localScale = Vector3.one * 0.9f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = cellSprite;
            sr.color = color;
            sr.sortingOrder = 10;
            cells.Add(sr);
        }
    }

    public void UpdateRotation(ParcelData data, int rotation, Color color)
    {
        Build(data, rotation, color);
    }

    public void SetAlpha(float alpha)
    {
        foreach (var sr in cells)
        {
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    public void SetColor(Color color)
    {
        foreach (var sr in cells)
            sr.color = color;
    }

    public void Clear()
    {
        foreach (var sr in cells)
            if (sr != null) Destroy(sr.gameObject);
        cells.Clear();
    }
}