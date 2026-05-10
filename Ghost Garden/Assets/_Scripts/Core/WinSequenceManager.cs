using UnityEngine;

public class WinSequenceManager : MonoBehaviour
{
    public static WinSequenceManager Instance;

    [Header("Win Condition State")]
    [SerializeField] bool _tarpDone;
    [SerializeField] bool _canDone;
    [SerializeField] bool _windchimesDone;

    [Header("Watering Can Window")]
    // Player can still knock the can over this many seconds after the
    // neighbour's walk starts. After this window the can nudge fails.
    public float wateringCanGracePeriod = 8f;

    void Awake() => Instance = this;

    // ── Public register methods ───────────────────────────────────────────────

    public void RegisterTarpNudge()
    {
        if (_tarpDone) return;
        _tarpDone = true;

        GameObject tarpObj = GameObject.FindWithTag("Tarp");
        if (tarpObj != null)
        {
            TarpAnimator ta = tarpObj.GetComponent<TarpAnimator>();
            if (ta != null) ta.BeginCarryAway();
            else            tarpObj.SetActive(false);
        }

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

        NeighbourAI neighbour = NeighbourAI.Instance;

        // Allow the nudge if the neighbour hasn't started walking yet,
        // OR if they have but the grace period hasn't expired yet.
        bool walkStarted = neighbour != null && neighbour.IsWalking;
        bool graceExpired = walkStarted && neighbour.WalkElapsed > wateringCanGracePeriod;

        if (graceExpired)
        {
            HUDManager.Instance?.ShowMessage("Too late — they've already walked past...");
            return;
        }

        _canDone = true;
        DropWateringCan();
        HUDManager.Instance?.ShowMessage("The watering can tumbles off the shelf...");
        Debug.Log($"[WinSequence] Watering can knocked over (walk elapsed: {neighbour?.WalkElapsed:F1}s).");
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
            anim.FallOff();
        else
        {
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