using UnityEngine;

public class ConveyorManager : MonoBehaviour
{
    [SerializeField] private ConveyorBelt beltPrefab;
    [SerializeField] private float beltOffset = 6f;

    private ConveyorBelt[] belts;

    private static readonly Color[] PlayerColors =
        { Color.red, Color.blue, Color.green, Color.yellow };

    // TL, TR, BL, BR
    private static readonly Vector2[] BeltPositions =
    {
        new Vector2(-1,  0.5f), // P1: left-top
        new Vector2( 1,  0.5f), // P2: right-top
        new Vector2(-1, -0.5f), // P3: left-bottom
        new Vector2( 1, -0.5f), // P4: right-bottom
    };

    // Belts point inward toward the truck
    // private static readonly float[] BeltRotations =
    //     { -45f, -135f, 45f, 135f };

    private static readonly float[] BeltRotations =
    { 0f, 0f, 0f, 0f };

    public void SetupBelts(int playerCount)
    {
        belts = new ConveyorBelt[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            Vector2 pos = BeltPositions[i] * beltOffset;
            var belt = Instantiate(beltPrefab, pos,
                Quaternion.identity, transform);
            belt.Initialize(i, PlayerColors[i]);
            belts[i] = belt;
        }
    }

    public void StartRound()
    {
        foreach (var belt in belts)
            belt.FillAllSlots();
    }

    public ConveyorBelt GetBelt(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= belts.Length) return null;
        return belts[playerIndex];
    }

    void Start()
    {
        // TODO: remove when GameManager exists
        SetupBelts(4);
        StartRound();
    }
}