using System;
using UnityEngine;

public class DepedencyManager : MonoBehaviour
{
    public static DepedencyManager Instance { get; private set; }
    public ConveyorManager conveyorManager;
    public GridManager gridManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        var players = FindObjectsByType<PlayerCursor>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            player.InjectDependencies(conveyorManager, gridManager);
        }
    }
}
