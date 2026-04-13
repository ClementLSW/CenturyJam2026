using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ConveyorManager conveyorManager;
    [SerializeField] private TruckTemplate[] truckTemplates;

    [Header("Round Settings")]
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float roundDuration = 60f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI[] playerScoreTexts; // one per player
    [SerializeField] private GameObject finalScorePanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private int currentRound = 0;
    private float timeRemaining;
    private bool roundActive = false;
    private bool playedTicking = false;
    private bool playedTimeOut = false;
    private int[] totalScores;
    private int playerCount;

    private static readonly Color[] PlayerColors =
        { Color.red, Color.blue, Color.green, Color.yellow };

    void Start()
    {
        finalScorePanel.SetActive(false);

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

        StartRound();
    }

    void Update()
    {
        if (!roundActive) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();


        if (timeRemaining <= 13f && !playedTicking)
        {
            playedTicking = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.ticking);
        }

        if (timeRemaining <= 1.5f && !playedTimeOut && currentRound == 3)
        {
            playedTimeOut = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.timeOut);
        }


        if (timeRemaining <= 0f)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.truckLeave);
            EndRound();
        }
    }

    private void StartRound()
    {
        currentRound++;

        // Pick a truck template
        int templateIndex = (currentRound - 1) % truckTemplates.Length;
        gridManager.LoadTemplate(truckTemplates[templateIndex]);

        conveyorManager.StartRound();
        AudioManager.Instance.PlaySFXDelayed(AudioManager.Instance.truckArrive, 1f);

        timeRemaining = roundDuration;
        roundActive = true;

        UpdateTimerUI();
        UpdateScoreUI();
    }

    private void EndRound()
    {
        roundActive = false;

        // Tally scores from grid
        TallyRoundScores();
        UpdateScoreUI();

        // Force drop any held parcels
        foreach (var handler in FindObjectsOfType<ParcelHandler>())
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
        for (int x = 0; x < gridManager.Width; x++)
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
        }
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

        string result = "Final scores\n\n";
        int winnerIndex = 0;
        for (int i = 0; i < playerCount; i++)
        {
            result += $"Player {i + 1}: {totalScores[i]} pts\n";
            if (totalScores[i] > totalScores[winnerIndex])
                winnerIndex = i;
        }
        result += $"\nPlayer {winnerIndex + 1} wins!";

        finalScoreText.text = result;
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