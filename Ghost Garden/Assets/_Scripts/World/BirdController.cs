// Assets/_Scripts/World/BirdController.cs
using UnityEngine;

public class BirdController : MonoBehaviour
{
    public static BirdController Instance;
    public GameObject[] birds;
    public Vector3 flyAwayDirection = Vector3.up + Vector3.forward;
    public float flySpeed = 5f;
    bool _scared;

    void Awake() => Instance = this;

    void Update()
    {
        if (!_scared) return;
        foreach (var b in birds)
            b.transform.position += flyAwayDirection * flySpeed * Time.deltaTime;
    }

    public void ScareAway()
    {
        _scared = true;
        // After 2s, trigger tarp reveal
        Invoke(nameof(TriggerTarp), 2f);
    }

    void TriggerTarp() =>
        WinSequenceManager.Instance?.RegisterNudge(1);
}