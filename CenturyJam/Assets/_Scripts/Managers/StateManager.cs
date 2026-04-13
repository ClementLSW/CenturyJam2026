using System;
using Unity.VisualScripting;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }
    private GameState _gameState;
    public GameState GetCurrentState() => _gameState;
    public void SetCurrentState(GameState gameState) => _gameState = gameState;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        }
        else
        {
            Instance = this;
        }
    }

    public enum GameState
    {
        MAINMENU,
        GAME,
    }
}
