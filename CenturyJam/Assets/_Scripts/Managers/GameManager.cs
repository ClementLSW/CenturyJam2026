using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GridManager gridManager;

    [SerializeField] private ConveyorManager conveyorManager;
    [SerializeField] private TruckTemplateGroup truckTemplateGroup;

    [Header("Round Settings")] [SerializeField]
    private int totalRounds = 3;

    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private int baseRoundScore = 1000;

    [Header("UI")] [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private TextMeshProUGUI[] playerScoreTexts; // one per player
    [SerializeField] private GameObject finalScorePanel;
    [SerializeField] private Animator finalScoreAnimator;
    [SerializeField] private TextMeshProUGUI finalScoreText1;
    [SerializeField] private TextMeshProUGUI finalScoreText2;
    [SerializeField] private TextMeshProUGUI finalScoreText3;
    [SerializeField] private TextMeshProUGUI finalScoreText4;
    [SerializeField] private GlobalVariable globalVariable;

    public GameObject Van;
    [SerializeField] private Sprite vanSpriteA;
    [SerializeField] private Sprite vanSpriteB;
    public Animator vanAnimator;
    public GameObject VanExhaust;
    private bool canReturnToMenu;

    private int currentRound;
    private bool playedTicking;
    private bool playedTimeOut;
    private bool playedVanLeave;
    private int playerCount;
    private float timeRemaining;
    private int[] totalScores;

    public bool IsRoundActive { get; private set; }

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        finalScorePanel.SetActive(false);
        canReturnToMenu = false;

        // TODO: replace with actual player count from lobby
        playerCount = PlayerInputManager.instance != null
            ? PlayerInputManager.instance.playerCount
            : 4;

        totalScores = new int[playerCount];
        conveyorManager.SetupBelts(playerCount);

        // Hide score texts for inactive players
        for (var i = 0; i < playerScoreTexts.Length; i++)
            if (playerScoreTexts[i] != null)
                playerScoreTexts[i].gameObject.SetActive(i < playerCount);

        AudioManager.Instance.PlaySFX(AudioManager.Instance
            .vanArrive); // pulling this here because it's delayed in rounds 2 and 3? -kl
        StartRound();
    }

    private void Update()
    {
        if (!IsRoundActive) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 13f && !playedTicking)
        {
            playedTicking = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.ticking);
        }

        if (timeRemaining <= 1.5f && !playedVanLeave)
        {
            playedVanLeave = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.vanLeave);
            VanExhaust.SetActive(true);

            if (currentRound == 3 && !playedTimeOut)
            {
                playedTimeOut = true;
                AudioManager.Instance.PlaySFX(AudioManager.Instance.timeOut);
            }

            Invoke(nameof(EndRound), 1f);
            return;
        }

        if (timeRemaining <= 0f) EndRound();
    }

    public void TryReturnToMenu()
    {
        if (!canReturnToMenu) return;
        finalScoreAnimator.Play("ClipboardExit");
        canReturnToMenu = false;
        StartCoroutine(MenuReturnDelay());
    }

    public IEnumerator MenuReturnDelay()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("MainMenu");
    }

    private void StartRound()
    {
        currentRound++;
        playedTicking = false;
        playedVanLeave = false;

        if (truckTemplateGroup == null)
        {
            Debug.LogError("TruckTemplateGroup not assigned in Inspector!");
            return;
        }

        // Pick a truck template
        //int groupIndex = (currentRound - 1) % truckTemplateGroup.GetCount();
        //int groupIndex = currentRound;

        var template = truckTemplateGroup.GetRandomTemplate();

        gridManager.LoadTemplate(template);
        Van.GetComponent<SpriteRenderer>().sprite = Random.Range(0, 2) == 0 ? vanSpriteA : vanSpriteB;

        conveyorManager.StartRound();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.vanArrive);
        vanAnimator.Play("VanArrive");
        VanExhaust.SetActive(false);

        timeRemaining = roundDuration;
        IsRoundActive = true;

        UpdateTimerUI();
        UpdateScoreUI();
    }

    private void EndRound()
    {
        CancelInvoke(nameof(EndRound));
        IsRoundActive = false;
        vanAnimator.Play("VanLeave");

        // Tally scores from grid
        TallyRoundScores();
        UpdateScoreUI();

        // Force drop any held parcels
        foreach (var handler in FindObjectsByType<ParcelHandler>(FindObjectsSortMode.None)) handler.CleanupHeld();

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
            player.GetComponent<ParcelHandler>().ClearBoardParcels();

        if (currentRound >= totalRounds)
            ShowFinalScores();
        else
            // Brief delay then next round
            Invoke(nameof(StartRound), 2f);
    }

    private void TallyRoundScores()
    {
        if (gridManager.Width == 0 || gridManager.Height == 0)
        {
            Debug.LogWarning("Grid not ready for scoring yet!");
            return;
        }

        var totalFilledCells = 0;
        int player1Cells = 0, player2Cells = 0, player3Cells = 0, player4Cells = 0;
        for (var x = 0; x < gridManager.Width; x++)
        for (var y = 0; y < gridManager.Height; y++)
        {
            var pos = new Vector2Int(x, y);
            var cell = gridManager.GetCell(pos);

            if (cell.state == CellState.Occupied)
            {
                totalFilledCells++;

                var data = gridManager.GetParcelDataAt(pos);
                if (data != null)
                    switch (cell.ownerID)
                    {
                        case 0:
                            player1Cells++;
                            break;
                        case 1:
                            player2Cells++;
                            break;
                        case 2:
                            player3Cells++;
                            break;
                        case 3:
                            player4Cells++;
                            break;
                    }
            }
        }

        //Divide by 0 check, if totalFilledCells is 0 just break, because there's no point calculating. No one did shit here
        if (totalFilledCells <= 0) return;

        //Calculate Penalty

        if (gridManager.Width == 0 || gridManager.Height == 0)
        {
            Debug.LogWarning("Grid not initialized properly!");
            return;
        }

        var totalGridSquares = gridManager.Height * gridManager.Width;
        var buffer = Mathf.FloorToInt(0.1f * totalGridSquares);
        var excessEmpty = Math.Max(totalGridSquares - totalFilledCells - buffer, 0);
        var penalty = excessEmpty / (float)totalGridSquares;
        var adjustedBaseScore = baseRoundScore * (1f - penalty);

        //Calculate individual color-ratio
        int[] playerCells = { player1Cells, player2Cells, player3Cells, player4Cells };
        for (var i = 0; i < playerCount; i++)
            totalScores[i] += Mathf.RoundToInt(adjustedBaseScore * (playerCells[i] / (float)totalFilledCells));

        //Outdated
        /*for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                CellData cell = gridManager.GetCell(pos);

                if (cell.state == CellState.Occupied)
                {
                    ParcelData data = gridManager.GetParcelDataAt(pos);
                    if (data != null && cell.ownerID < totalScores.Length)
                    {
                        // Only count once per parcel, not per cell
                        // Check if this is the first cell of this parcel
                        if (IsFirstCellOfParcel(pos, cell.parcelID))
                        {
                            totalScores[cell.ownerID] += data.pointValue;
                            AudioManager.Instance.PlaySFX(AudioManager.Instance.scoreIncrease);
                        }
                    }
                }
            }
        }*/
    }

    private bool IsFirstCellOfParcel(Vector2Int pos, int parcelId)
    {
        // Check if there's any cell with the same parcelId
        // that comes before this one in scan order
        for (var x = 0; x < gridManager.Width; x++)
        for (var y = 0; y < gridManager.Height; y++)
        {
            if (x == pos.x && y == pos.y) return true;
            var cell = gridManager.GetCell(new Vector2Int(x, y));
            if (cell.parcelID == parcelId) return false;
        }

        return true;
    }

    private void ShowFinalScores()
    {
        finalScorePanel.SetActive(true);
        canReturnToMenu = true;
        finalScoreAnimator.Play("ClipboardEnter");

        var result = "Final scores\n\n";
        var winnerIndex = 0;
        for (var i = 0; i < playerCount; i++)
        {
            result += $"Player {i + 1}: {totalScores[i]} pts\n";
            if (totalScores[i] > totalScores[winnerIndex])
                winnerIndex = i;
        }

        result += $"\nPlayer {winnerIndex + 1} wins!";

        //finalScoreText.text = result;
        if (totalScores.Length > 0) finalScoreText1.text = $"Player 1: {totalScores[0]}";
        if (totalScores.Length > 1) finalScoreText2.text = $"Player 2: {totalScores[1]}";
        if (totalScores.Length > 2) finalScoreText3.text = $"Player 3: {totalScores[2]}";
        if (totalScores.Length > 3) finalScoreText4.text = $"Player 4: {totalScores[3]}";
        AudioManager.Instance.PlaySFX(AudioManager.Instance.payout);

        // TODO: restart or return to menu on button press
    }

    private void UpdateTimerUI()
    {
        var seconds = Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
        timerText.text = seconds.ToString();

        if (timeRemaining <= 15f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;
    }

    private void UpdateScoreUI()
    {
        for (var i = 0; i < playerCount; i++)
            if (i < playerScoreTexts.Length && playerScoreTexts[i] != null)
            {
                playerScoreTexts[i].text = $"P{i + 1}: {totalScores[i]}";
                switch (i)
                {
                    case 0:
                        playerScoreTexts[i].color = globalVariable.player1Color;
                        break;
                    case 1:
                        playerScoreTexts[i].color = globalVariable.player2Color;
                        break;
                    case 2:
                        playerScoreTexts[i].color = globalVariable.player3Color;
                        break;
                    case 3:
                        playerScoreTexts[i].color = globalVariable.player4Color;
                        break;
                }
            }
    }
}