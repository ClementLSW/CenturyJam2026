using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCursor : MonoBehaviour
{
    [SerializeField] private CursorSettings cursorSettings;


    private Vector2 moveInput;
    private int playerIndex;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HandleInteract();
    }

    public void OnRotateCW(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HandleRotateCW();
    }

    public void OnRotateCCW(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HandleRotateCCW();
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HandlePause();
    }

    void Start()
    {
        playerIndex = GetComponent<PlayerInput>().playerIndex;
        // Use playerIndex to assign color, belt, etc.
    }

    void Update()
    {
        transform.position += (Vector3)(moveInput * cursorSettings.MoveSpeed * Time.deltaTime);
    }

    void HandleInteract() {
        //TODO: Handle Interaction
        Debug.Log("Interacted with object");
    }
    void HandleRotateCW() { 
        // TODO: Handle Rotation Clockwise
        Debug.Log("Rotated Clockwise");
     }
    void HandleRotateCCW() { 
        // TODO: Handle Rotation Counter-Clockwise
        Debug.Log("Rotated Counter-Clockwise");
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