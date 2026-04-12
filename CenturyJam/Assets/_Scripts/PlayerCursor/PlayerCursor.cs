using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCursor : MonoBehaviour
{
    [SerializeField] private CursorSettings cursorSettings;


    private Vector2 moveInput;
    private ParcelHandler parcelHandler;
    public int PlayerIndex { get; private set; }
    public Color PlayerColor { get; private set; }

    private static readonly Color[] Colors = { Color.red, Color.blue, Color.green, Color.yellow };

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) parcelHandler.HandleInteract();
    }

    public void OnRotateCW(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) parcelHandler.HandleRotateCW();
    }

    public void OnRotateCCW(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) parcelHandler.HandleRotateCCW();
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HandlePause();
    }

    void Start()
    {
        PlayerIndex = GetComponent<PlayerInput>().playerIndex;
        PlayerColor = Colors[PlayerIndex % Colors.Length];
        parcelHandler = GetComponent<ParcelHandler>();
    }

    void Update()
    {
        transform.position += (Vector3)(moveInput * cursorSettings.MoveSpeed * Time.deltaTime);
    }

    void HandlePause() {
        // TODO: Handle Pause
        if (Time.timeScale == 0) {
            Time.timeScale = 1;
        } else {
            Time.timeScale = 0;
        }
        Debug.Log("Game Paused");
    }
}