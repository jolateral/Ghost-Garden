using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed        = 4f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Debug")]
    public bool debugFootsteps = true;

    public bool InputDisabled { get; private set; }

    public void SetInputEnabled(bool enabled)
    {
        InputDisabled = !enabled;

        if (InputDisabled)
            AudioManager.Instance?.SetPlayerFootstepsActive(false, transform.position);
    }

    CharacterController _cc;
    float _xRotation;
    InputAction _moveAction;
    InputAction _lookAction;
    bool _wasMoving;

    void Awake()
    {
        _moveAction = new InputAction("Move");
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _lookAction = new InputAction("Look", binding: "<Mouse>/delta");
    }

    void OnEnable()
    {
        _moveAction.Enable();
        _lookAction.Enable();
    }

    void OnDisable()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        AudioManager.Instance?.SetPlayerFootstepsActive(false, transform.position);
    }

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (debugFootsteps)
        {
            if (AudioManager.Instance == null)
                Debug.LogError("[PlayerController] AudioManager.Instance is NULL!");
            else
                Debug.Log("[PlayerController] AudioManager found OK.");
        }
    }

    void Update()
    {
        if (InputDisabled)
        {
            HandleLook();
            return;
        }

        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        Vector2 look = _lookAction.ReadValue<Vector2>();
        float mouseX = look.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime * 100f;

        _xRotation = Mathf.Clamp(_xRotation + mouseY, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        Vector3 move  = transform.right * input.x + transform.forward * input.y;
        _cc.Move(move * moveSpeed * Time.deltaTime);

        bool isMoving = input.magnitude > 0.1f;

        if (debugFootsteps && isMoving != _wasMoving)
            Debug.Log($"[PlayerController] Movement state changed → isMoving: {isMoving}, input magnitude: {input.magnitude:F3}");

        _wasMoving = isMoving;
        AudioManager.Instance?.SetPlayerFootstepsActive(isMoving, transform.position);
    }
}