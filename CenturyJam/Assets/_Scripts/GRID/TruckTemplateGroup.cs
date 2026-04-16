using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TruckTemplateGroup", menuName = "Game/Truck Template Group")]
public class TruckTemplateGroup : ScriptableObject
{
    public List<TruckTemplate> templates = new List<TruckTemplate>();

    public TruckTemplate GetRandomTemplate()
    {
        if (templates == null || templates.Count == 0)
        {
            Debug.LogError("No templates assigned!");
            return null;
        }
        else
        {
            return templates[Random.Range(0, templates.Count - 1)];
        }
    }

    public int GetCount()
    {
        return templates.Count;
    }
}