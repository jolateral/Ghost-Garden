using UnityEngine;

public class GhostInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Camera playerCamera;
    public GameObject crosshairDefault;
    public GameObject crosshairInteract;

    NudgeableObject _hovering;

    void Update()
    {
        CheckHover();
        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            TryNudge();
    }

    void CheckHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
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

        // TrySpendNudge shows the "no nudges left" message automatically
        if (!NudgeSystem.Instance.TrySpendNudge()) return;

        _hovering.Nudge();
    }
}