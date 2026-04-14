using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCursor : MonoBehaviour
{
    [SerializeField] private CursorSettings cursorSettings;


    private Vector2 moveInput;
    private ParcelHandler parcelHandler;
    public int PlayerIndex { get; private set; }
    private Color _playerColor;
    public Color PlayerColor => _playerColor;
    private SpriteRenderer _cursorSprite;
    
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (StateManager.Instance == null) return;
        if (ctx.performed && StateManager.Instance.GetCurrentState() == StateManager.GameState.MAINMENU) PlayerEnrolmentManager.Instance.TryStartGame();
        if (parcelHandler == null) return;
        if (ctx.performed && StateManager.Instance.GetCurrentState() == StateManager.GameState.GAME) parcelHandler.HandleInteract();
    }

    public void OnRotateCW(InputAction.CallbackContext ctx)
    {
        if (StateManager.Instance == null) return;
        if (parcelHandler == null) return;
        if (ctx.performed && StateManager.Instance.GetCurrentState() == StateManager.GameState.GAME) parcelHandler.HandleRotateCW();
    }

    public void OnRotateCCW(InputAction.CallbackContext ctx)
    {
        if (StateManager.Instance == null) return;
        if (parcelHandler == null) return;
        if (ctx.performed && StateManager.Instance.GetCurrentState() == StateManager.GameState.GAME) parcelHandler.HandleRotateCCW();
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HandlePause();
    }

    private void Awake()
    {
        _cursorSprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        PlayerIndex = GetComponent<PlayerInput>().playerIndex;
        parcelHandler = GetComponent<ParcelHandler>();
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerColor(Color color)
    {
        _playerColor = color;
        StartCoroutine(WaitForSpriteRendererAndSetColor());
        IEnumerator WaitForSpriteRendererAndSetColor()
        {
            while (_cursorSprite ==null) yield return null;
            _cursorSprite.color = _playerColor;
        }
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