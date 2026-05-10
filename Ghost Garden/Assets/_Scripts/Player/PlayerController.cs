using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    CharacterController _cc;
    float _xRotation;

    // New Input System action references
    InputAction _moveAction;
    InputAction _lookAction;

    void Awake()
    {
        // Create actions bound to the same controls as the old Input class used
        _moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        _lookAction = new InputAction("Look", binding: "<Mouse>/delta");
        _lookAction.AddBinding("<Gamepad>/rightStick");
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
    }

    void Start()
    {
        _cc = gameObject.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse look
        Vector2 look = _lookAction.ReadValue<Vector2>();
        float mouseX = look.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime * 100f;

        _xRotation = Mathf.Clamp(_xRotation - mouseY, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // WASD movement
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right   * moveInput.x
                     + transform.forward * moveInput.y;
        _cc.Move(move * moveSpeed * Time.deltaTime);
    }
}