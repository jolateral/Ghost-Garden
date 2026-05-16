using UnityEngine;
using UnityEngine.InputSystem;

public class GhostInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Camera playerCamera;
    public GameObject crosshairDefault;
    public GameObject crosshairInteract;

    [Tooltip("Set this to everything EXCEPT the Player layer so the raycast doesn't hit your own collider.")]
    public LayerMask interactMask = Physics.DefaultRaycastLayers;

    NudgeableObject _hovering;
    InputAction _nudgeAction;

    void Awake()
    {
        _nudgeAction = new InputAction("Nudge");
        _nudgeAction.AddBinding("<Keyboard>/e");
        _nudgeAction.AddBinding("<Mouse>/leftButton");
    }

    void OnEnable()
    {
        _nudgeAction.Enable();
        _nudgeAction.performed += OnNudgePerformed;
    }

    void OnDisable()
    {
        _nudgeAction.performed -= OnNudgePerformed;
        _nudgeAction.Disable();
    }

    void OnNudgePerformed(InputAction.CallbackContext ctx)
    {
        TryNudge();
    }

    void Update()
    {
        CheckHover();
    }

    void CheckHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        // interactMask excludes the Player layer so the ray never hits your own collider
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
        {
            _hovering = hit.collider.gameObject.GetComponent<NudgeableObject>();
        }
        else
        {
            _hovering = null;
        }

        crosshairDefault.SetActive(_hovering == null);
        crosshairInteract.SetActive(_hovering != null);
    }

    void TryNudge()
    {
        if (_hovering == null) return;
        if (GameManager.Instance.gameWon) return;
        if (!NudgeSystem.Instance.TrySpendNudge()) return;
        _hovering.Nudge();
    }
}