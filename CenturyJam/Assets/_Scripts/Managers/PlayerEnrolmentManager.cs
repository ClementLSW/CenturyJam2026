using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEnrolmentManager : MonoBehaviour
{
    public static PlayerEnrolmentManager Instance;
    [SerializeField] private GlobalVariable globalVariable;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<PlayerEnrolmentIndicator> playerEnrolmentIndicators;
    [SerializeField] private List<ConveyorMenuVisual> conveyors;
    [SerializeField] private TextMeshProUGUI gameStartStatus;
    private readonly Dictionary<PlayerInput, int> _players = new();
    private bool _allowGameStart;
    private Coroutine _countdownCoroutine;
    private SceneChangeManager _sceneChangeManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
        _sceneChangeManager = GetComponent<SceneChangeManager>();
    }

    private void Start()
    {
        playerEnrolmentIndicators[0].SetIndicatorColor(globalVariable.player1Color);
        playerEnrolmentIndicators[1].SetIndicatorColor(globalVariable.player2Color);
        playerEnrolmentIndicators[2].SetIndicatorColor(globalVariable.player3Color);
        playerEnrolmentIndicators[3].SetIndicatorColor(globalVariable.player4Color);
        gameStartStatus.text = $"Waiting For More Players... {_players.Count}/4.";
        
    }

    public void PlayerJoined(PlayerInput player)
    {
        var newPlayerId = AddPlayerToPool(player);
        if (newPlayerId != -1)
        {
            MovePlayerToStartPosition(player.transform, newPlayerId);
            SetPlayerCursorColor(player, newPlayerId);
            ActivateEnrolmentIndicator(newPlayerId);
        }
        _allowGameStart = _players.Count >= 2;
        gameStartStatus.text = _players.Count >=2?$"Waiting For More Players... {_players.Count}/4. Press A to Start Game.":$"Waiting For More Players... {_players.Count}/4.";
    }

    public void PlayerLeft(PlayerInput player)
    {
        _players.Remove(player, out var removedPlayerId);
        conveyors[removedPlayerId].Disappear();
        DeactivateEnrolmentIndicator(removedPlayerId);
        gameStartStatus.text = _players.Count >=2?$"Waiting For More Players... {_players.Count}/4. Press A to Start Game.":$"Waiting For More Players... {_players.Count}/4.";
        _allowGameStart = _players.Count >= 2;
        if (!_allowGameStart) HaltCountdown();
    }

    public void TryStartGame()
    {
        if (_allowGameStart && _countdownCoroutine == null)
            _countdownCoroutine = StartCoroutine(CountdownToStartGame());
    }

    private int GetFirstAvailablePlayerId()
    {
        for (var i = 0; i < 4; i++)
            if (!_players.ContainsValue(i))
                return i;

        return -1;
    }

    private int AddPlayerToPool(PlayerInput player)
    {
        var playerId = GetFirstAvailablePlayerId();
        if (playerId == -1)
        {
            Debug.LogError("No available slots, failed to add player");
            return -1;
        }

        conveyors[playerId].Appear();
        _players.Add(player, playerId);
        return playerId;
    }

    private void MovePlayerToStartPosition(Transform player, int playerId)
    {
        player.transform.position = spawnPoints[playerId].position;
    }

    private void SetPlayerCursorColor(PlayerInput player, int playerId)
    {
        Color playerColor = default;
        switch (playerId)
        {
            case 0:
                playerColor = globalVariable.player1Color;
                break;
            case 1:
                playerColor = globalVariable.player2Color;
                break;
            case 2:
                playerColor = globalVariable.player3Color;
                break;
            case 3:
                playerColor = globalVariable.player4Color;
                break;
        }

        player.GetComponent<PlayerCursor>().SetPlayerColor(playerColor);
    }

    private void ActivateEnrolmentIndicator(int playerId)
    {
        playerEnrolmentIndicators[playerId].ActivateIndicator();
    }

    private void DeactivateEnrolmentIndicator(int playerId)
    {
        playerEnrolmentIndicators[playerId].DeactivateIndicator();
    }

    private IEnumerator CountdownToStartGame()
    {
        var time = 6f;
        while (time > 0)
        {
            gameStartStatus.text = $"Game Starting In {Mathf.FloorToInt(time)}s";
            time -= Time.deltaTime;
            yield return null;
        }
        
        _sceneChangeManager.StartGame();
    }

    private void HaltCountdown()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }

        gameStartStatus.text = "Waiting For More Players...";
    }
}