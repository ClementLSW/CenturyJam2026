using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ConveyorManager conveyorManager;
    [SerializeField] private TruckTemplate[] truckTemplates;

    [Header("Round Settings")]
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private int baseRoundScore = 1000;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI[] playerScoreTexts; // one per player
    [SerializeField] private GameObject finalScorePanel;
    [SerializeField] private Animator finalScoreAnimator;
    private bool canReturnToMenu = false;
    [SerializeField] private TextMeshProUGUI finalScoreText1;
    [SerializeField] private TextMeshProUGUI finalScoreText2;
    [SerializeField] private TextMeshProUGUI finalScoreText3;
    [SerializeField] private TextMeshProUGUI finalScoreText4;

    private int currentRound = 0;
    private float timeRemaining;
    private bool roundActive = false;
    private bool playedTicking = false;
    private bool playedTimeOut = false;
    private bool playedVanLeave = false;
    private int[] totalScores;
    private int playerCount;

    public GameObject Van;
    [SerializeField] private Sprite vanSpriteA;
    [SerializeField] private Sprite vanSpriteB;
    public Animator vanAnimator;
    public GameObject VanExhaust;


    private static readonly Color[] PlayerColors =
        { Color.red, Color.blue, Color.green, Color.yellow };

    void Start()
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
        for (int i = 0; i < playerScoreTexts.Length; i++)
        {
            if (playerScoreTexts[i] != null)
                playerScoreTexts[i].gameObject.SetActive(i < playerCount);
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.vanArrive); // pulling this here because it's delayed in rounds 2 and 3? -kl
        StartRound();
    }

    void Update()
    {
        if (canReturnToMenu && Input.GetKeyDown(KeyCode.E))
        {
            finalScoreAnimator.Play("ClipboardExit");
            canReturnToMenu = false;
            Debug.Log("Returning to menu...");
            StartCoroutine(MenuReturnDelay());
        }

        if (!roundActive) return;

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

        if (timeRemaining <= 0f)
        {
            EndRound();
        }
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

        // Pick a truck template
        int templateIndex = (currentRound - 1) % truckTemplates.Length;
        //gridManager.LoadTemplate(truckTemplates[templateIndex]);
        gridManager.FadeOutAndReload(truckTemplates[templateIndex]);
        Van.GetComponent<SpriteRenderer>().sprite = UnityEngine.Random.Range(0, 2) == 0 ? vanSpriteA : vanSpriteB;

        conveyorManager.StartRound();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.vanArrive);
        vanAnimator.Play("VanArrive");
        VanExhaust.SetActive(false);

        timeRemaining = roundDuration;
        roundActive = true;

        UpdateTimerUI();
        UpdateScoreUI();
    }

    private void EndRound()
    {
        CancelInvoke(nameof(EndRound));
        roundActive = false;
        vanAnimator.Play("VanLeave");

        // Tally scores from grid
        TallyRoundScores();
        UpdateScoreUI();

        // Force drop any held parcels
        foreach (var handler in FindObjectsByType<ParcelHandler>(FindObjectsSortMode.None))
        {
            handler.CleanupHeld();
        }

        if (currentRound >= totalRounds)
        {
            ShowFinalScores();
        }
        else
        {
            // Brief delay then next round
            Invoke(nameof(StartRound), 2f);
        }
    }

    private void TallyRoundScores()
    {
        int totalFilledCells = 0;
        int player1Cells = 0, player2Cells = 0, player3Cells = 0, player4Cells = 0;
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                CellData cell = gridManager.GetCell(pos);

                if (cell.state == CellState.Occupied)
                {
                    totalFilledCells++;
                    
                    ParcelData data = gridManager.GetParcelDataAt(pos);
                    if (data != null)
                    {
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
            }
        }
        //Divide by 0 check, if totalFilledCells is 0 just break, because there's no point calculating. No one did shit here
        if (totalFilledCells <= 0) return;
        
        //Calculate Penalty
        var totalGridSquares = gridManager.Height * gridManager.Width;
        var buffer = Mathf.FloorToInt(0.1f * totalGridSquares);
        var excessEmpty = Math.Max(totalGridSquares - totalFilledCells - buffer, 0);
        var penalty = ((float)excessEmpty / (float)totalGridSquares);
        var adjustedBaseScore = baseRoundScore * (1f - penalty);

        //Calculate individual color-ratio
        int[] playerCells = { player1Cells, player2Cells, player3Cells, player4Cells };
        for (int i = 0; i < playerCount; i++)
            totalScores[i] += Mathf.RoundToInt(adjustedBaseScore * ((float)playerCells[i] / (float)totalFilledCells));

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
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
            {
                if (x == pos.x && y == pos.y) return true;
                CellData cell = gridManager.GetCell(new Vector2Int(x, y));
                if (cell.parcelID == parcelId) return false;
            }
        }
        return true;
    }

    private void ShowFinalScores()
    {
        finalScorePanel.SetActive(true);
        canReturnToMenu = true;
        finalScoreAnimator.Play("ClipboardEnter");

        string result = "Final scores\n\n";
        int winnerIndex = 0;
        for (int i = 0; i < playerCount; i++)
        {
            result += $"Player {i + 1}: {totalScores[i]} pts\n";
            if (totalScores[i] > totalScores[winnerIndex])
                winnerIndex = i;
        }
        result += $"\nPlayer {winnerIndex + 1} wins!";

        //finalScoreText.text = result;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.payout);

        // TODO: restart or return to menu on button press
    }

    private void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
        timerText.text = seconds.ToString();

        if (timeRemaining <= 15f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;
    }

    private void UpdateScoreUI()
    {
        for (int i = 0; i < playerCount; i++)
        {
            if (i < playerScoreTexts.Length && playerScoreTexts[i] != null)
            {
                playerScoreTexts[i].text = $"P{i + 1}: {totalScores[i]}";
                playerScoreTexts[i].color = PlayerColors[i];
            }
        }
    }

    public bool IsRoundActive => roundActive;
}