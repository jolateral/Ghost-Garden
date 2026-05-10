using UnityEngine;

// Tracks the three win conditions independently.
// Order of completion:
//   1. Tarp nudged (any time)             -> birds fly away, damaged garden exposed
//   2. Watering can nudged (before walk)  -> can falls into neighbour's path
//   3. Windchimes nudged (neighbour nearby) -> neighbour notices garden -> win

public class WinSequenceManager : MonoBehaviour
{
    public static WinSequenceManager Instance;

    [Header("Win Condition State (read-only in Inspector)")]
    [SerializeField] bool _tarpDone;
    [SerializeField] bool _canDone;
    [SerializeField] bool _windchimesDone;

    void Awake() => Instance = this;

    // ─── Called by NudgeableObject ───────────────────────────────────────────

    public void RegisterTarpNudge()
    {
        if (_tarpDone) return;

        _tarpDone = true;
        BirdController.Instance?.ScareAway(); // birds fly off and pull the tarp
        HUDManager.Instance?.ShowMessage("The birds scatter...");
        Debug.Log("[WinSequence] Tarp nudged — birds flying away.");
    }

    public void RegisterWateringCanNudge()
    {
        if (_canDone)
        {
            HUDManager.Instance?.ShowMessage("The can is already on the ground.");
            return;
        }

        // Must happen before the neighbour's walk starts
        if (NeighbourAI.Instance != null && NeighbourAI.Instance.IsWalking)
        {
            HUDManager.Instance?.ShowMessage("Too late — they've already walked past...");
            return;
        }

        _canDone = true;
        DropWateringCan();
        HUDManager.Instance?.ShowMessage("The watering can tumbles off the shelf...");
        Debug.Log("[WinSequence] Watering can knocked over.");
    }

    public void RegisterWindchimeNudge()
    {
        if (_windchimesDone) return;

        if (!_canDone)
        {
            HUDManager.Instance?.ShowMessage("Nothing seems to happen...");
            return;
        }

        // Neighbour must be walking and within range
        NeighbourAI neighbour = NeighbourAI.Instance;
        if (neighbour == null || !neighbour.IsWalking)
        {
            HUDManager.Instance?.ShowMessage("There's no one to hear it...");
            return;
        }

        if (!neighbour.IsNearHouse)
        {
            HUDManager.Instance?.ShowMessage("They're too far away to notice...");
            return;
        }

        _windchimesDone = true;
        PlayWindchimes();
        HUDManager.Instance?.ShowMessage("The chimes ring out...");
        Debug.Log("[WinSequence] Windchimes hit — triggering win.");

        // Small delay so the chime sound plays before win screen
        Invoke(nameof(TriggerWin), 1.5f);
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    void DropWateringCan()
    {
        GameObject canObj = GameObject.FindWithTag("WateringCan");
        if (canObj == null)
        {
            Debug.LogWarning("[WinSequence] No GameObject tagged 'WateringCan' found.");
            return;
        }

        // Animate the can falling off the shelf
        WateringCanAnimator animator = canObj.GetComponent<WateringCanAnimator>();
        if (animator != null)
        {
            animator.FallOff();
        }
        else
        {
            // Fallback: just enable physics if no animator present
            Rigidbody rb = canObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                // Give it a small nudge so it tips rather than drops straight down
                rb.AddForce(Vector3.forward * 1.5f + Vector3.right * 0.5f, ForceMode.Impulse);
            }
        }
    }

    void PlayWindchimes()
    {
        GameObject chimeObj = GameObject.FindWithTag("Windchimes");
        if (chimeObj == null) return;

        AudioSource chimes = chimeObj.GetComponent<AudioSource>();
        if (chimes != null) chimes.Play();

        // Gently sway the windchime object
        WindchimeAnimator wa = chimeObj.GetComponent<WindchimeAnimator>();
        if (wa != null) wa.Sway();
    }

    void TriggerWin()
    {
        NeighbourAI.Instance?.NoticeGarden();
    }

    // ─── Public getters (used by NeighbourAI / other systems) ────────────────

    public bool TarpDone      => _tarpDone;
    public bool CanDone       => _canDone;
    public bool WindchimesDone => _windchimesDone;
}