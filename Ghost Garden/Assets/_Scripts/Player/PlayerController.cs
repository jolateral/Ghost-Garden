using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    CharacterController _cc;
    float _xRotation;

    void Start()
    {
        _cc = gameObject.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        _xRotation = Mathf.Clamp(_xRotation - mouseY, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // WASD movement
        Vector3 move = transform.right   * Input.GetAxis("Horizontal")
                     + transform.forward * Input.GetAxis("Vertical");
        _cc.Move(move * moveSpeed * Time.deltaTime);
    }
}