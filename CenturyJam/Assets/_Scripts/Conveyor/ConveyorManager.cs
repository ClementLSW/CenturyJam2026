using UnityEngine;
using System.Collections.Generic;

public class ConveyorManager : MonoBehaviour
{
    public PlayerConveyorGroup[] players;

    [SerializeField] private GlobalVariable globalVariable;

    void Awake()
    {
        players = FindObjectsByType<PlayerConveyorGroup>(FindObjectsSortMode.None);
    }

    public void SetupBelts(int playerCount)
    {
        // Optional safety check
        if (players.Length < playerCount)
        {
            Debug.LogError("Not enough PlayerConveyorGroup objects in scene!");
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (i < playerCount)
            {
                players[i].gameObject.SetActive(true);
                switch (i)
                {
                    case 0:
                        players[i].Initialize(i, globalVariable.player1Color);
                        break;
                    case 1:
                        players[i].Initialize(i, globalVariable.player2Color);
                        break;
                    case 2:
                        players[i].Initialize(i, globalVariable.player3Color);
                        break;
                    case 3:
                        players[i].Initialize(i, globalVariable.player4Color);
                        break;
                }
            }
            else
            {
                players[i].gameObject.SetActive(false);
            }
        }
    }

    public void StartRound()
    {
        foreach (var player in players)
        {
            if (!player.gameObject.activeSelf) continue;
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
            if (!player.gameObject.activeSelf) continue;
            if (player.PlayerID == playerID)
                return new List<ConveyorBelt>(player.GetBelts());
        }

        return new List<ConveyorBelt>();
    }
}