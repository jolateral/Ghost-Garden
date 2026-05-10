using UnityEngine;

public class WinSequenceManager : MonoBehaviour
{
    public static WinSequenceManager Instance;

    [Header("Win Condition State")]
    [SerializeField] bool _tarpDone;
    [SerializeField] bool _canDone;
    [SerializeField] bool _windchimesDone;

    void Awake() => Instance = this;

    // ── Public register methods (called by NudgeableObject) ──────────────────

    public void RegisterTarpNudge()
    {
        if (_tarpDone) return;
        _tarpDone = true;

        // Find and animate the tarp
        GameObject tarpObj = GameObject.FindWithTag("Tarp");
        if (tarpObj != null)
        {
            TarpAnimator ta = tarpObj.GetComponent<TarpAnimator>();
            if (ta != null)
                ta.BeginCarryAway();
            else
                tarpObj.SetActive(false); // fallback
        }

        // Tell the birds to fly (BirdController also triggers TarpAnimator,
        // but calling both is safe — BirdController checks _scared flag)
        BirdController.Instance?.ScareAway();

        HUDManager.Instance?.ShowMessage("The birds scatter...");
        Debug.Log("[WinSequence] Tarp nudged.");
    }

    public void RegisterWateringCanNudge()
    {
        if (_canDone)
        {
            HUDManager.Instance?.ShowMessage("The can is already on the ground.");
            return;
        }

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
        Debug.Log("[WinSequence] Windchimes triggered — win incoming.");
        Invoke(nameof(TriggerWin), 1.5f);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    void DropWateringCan()
    {
        GameObject canObj = GameObject.FindWithTag("WateringCan");
        if (canObj == null)
        {
            Debug.LogWarning("[WinSequence] No 'WateringCan' tagged object found.");
            return;
        }

        WateringCanAnimator anim = canObj.GetComponent<WateringCanAnimator>();
        if (anim != null)
        {
            anim.FallOff();
        }
        else
        {
            // Fallback if animator not present
            Rigidbody rb = canObj.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        }
    }

    void PlayWindchimes()
    {
        GameObject chimeObj = GameObject.FindWithTag("Windchimes");
        if (chimeObj == null) return;

        WindchimeAnimator wa = chimeObj.GetComponent<WindchimeAnimator>();
        if (wa != null) wa.Sway();
    }

    void TriggerWin() => NeighbourAI.Instance?.NoticeGarden();

    public bool TarpDone       => _tarpDone;
    public bool CanDone        => _canDone;
    public bool WindchimesDone => _windchimesDone;
}