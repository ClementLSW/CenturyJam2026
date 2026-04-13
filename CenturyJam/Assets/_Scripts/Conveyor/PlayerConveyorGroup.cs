using UnityEngine;

public class PlayerConveyorGroup : MonoBehaviour
{
    public int PlayerID { get; private set; }

    private ConveyorBelt[] belts;

    public void Initialize(int playerID, Color color)
    {
        PlayerID = playerID;

        belts = GetComponentsInChildren<ConveyorBelt>();

        foreach (var belt in belts)
        {
            belt.Initialize(playerID, color);
        }
    }

    public ConveyorBelt[] GetBelts()
    {
        return belts;
    }
}
