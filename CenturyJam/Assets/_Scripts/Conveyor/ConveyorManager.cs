using UnityEngine;
using System.Collections.Generic;

public class ConveyorManager : MonoBehaviour
{
    public PlayerConveyorGroup[] players;

    private static readonly Color[] PlayerColors =
        { Color.red, Color.blue, Color.green, Color.yellow };

    void Awake()
    {
        players = FindObjectsOfType<PlayerConveyorGroup>();
    }

    public void SetupBelts(int playerCount)
    {
        // Optional safety check
        if (players.Length < playerCount)
        {
            Debug.LogError("Not enough PlayerConveyorGroup objects in scene!");
            return;
        }

        for (int i = 0; i < playerCount; i++)
        {
            players[i].Initialize(i, PlayerColors[i]);
        }
    }

    public void StartRound()
    {
        foreach (var player in players)
        {
            foreach (var belt in player.GetBelts())
            {
                belt.SpawnParcel();
            }
        }
    }

    public void NotifyParcelPlaced(int playerID)
    {
        var belts = GetBelts(playerID);

        foreach (var belt in belts)
        {
            if (belt.HasParcelReady()) continue;
            belt.SpawnParcel();
        }
    }

    public List<ConveyorBelt> GetBelts(int playerID)
    {
        foreach (var player in players)
        {
            if (player.PlayerID == playerID)
                return new List<ConveyorBelt>(player.GetBelts());
        }

        return new List<ConveyorBelt>();
    }
}