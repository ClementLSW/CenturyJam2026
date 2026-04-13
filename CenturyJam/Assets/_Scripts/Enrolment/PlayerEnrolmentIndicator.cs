using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnrolmentIndicator : MonoBehaviour
{
    private Image indicator;
    private Color indicatorColor;

    private void Awake()
    {
        indicator = GetComponent<Image>();
    }

    public void SetIndicatorColor(Color color)
    {
        indicatorColor = color;
    }

    public void ActivateIndicator()
    {
        indicator.color = indicatorColor;
    }

    public void DeactivateIndicator()
    {
        indicator.color = Color.white;
    }
}
