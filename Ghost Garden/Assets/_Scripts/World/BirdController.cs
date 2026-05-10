using UnityEngine;
using System.Collections;

public class BirdController : MonoBehaviour
{
    public static BirdController Instance;

    [Header("Birds")]
    public GameObject[] birds;
    public float flySpeed     = 4f;
    public float riseHeight   = 10f;
    public float flutterAmount = 0.15f;

    [Header("References")]
    public TarpAnimator tarpAnimator;

    bool _scared;

    void Awake() => Instance = this;

    public void ScareAway()
    {
        if (_scared) return;
        _scared = true;

        // Stop each bird's idle hopping before the fly-away starts
        foreach (var b in birds)
        {
            if (b == null) continue;
            BirdIdleHop hop = b.GetComponent<BirdIdleHop>();
            if (hop != null) hop.StopHopping();
        }

        AudioManager.Instance?.PlayBirdChirp(transform.position);
        tarpAnimator?.BeginCarryAway();
        StartCoroutine(FlyBirdsAway());
    }

    IEnumerator FlyBirdsAway()
    {
        // Give each bird a slightly different escape direction
        Vector3[] targets = new Vector3[birds.Length];
        for (int i = 0; i < birds.Length; i++)
        {
            if (birds[i] == null) continue;
            float angle = (360f / birds.Length) * i + Random.Range(-20f, 20f);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            targets[i] = birds[i].transform.position
                       + dir * 15f
                       + Vector3.up * riseHeight;
        }

        float elapsed  = 0f;
        float duration = riseHeight / flySpeed;

        Vector3[] starts = new Vector3[birds.Length];
        for (int i = 0; i < birds.Length; i++)
            if (birds[i] != null) starts[i] = birds[i].transform.position;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            for (int i = 0; i < birds.Length; i++)
            {
                if (birds[i] == null) continue;
                Vector3 pos = Vector3.Lerp(starts[i], targets[i], t);
                pos.x += Mathf.Sin(Time.time * 8f + i) * flutterAmount;
                pos.z += Mathf.Cos(Time.time * 7f + i) * flutterAmount;
                birds[i].transform.position = pos;

                Vector3 dir = (targets[i] - starts[i]).normalized;
                if (dir != Vector3.zero)
                    birds[i].transform.rotation = Quaternion.LookRotation(dir);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var b in birds)
            if (b != null) b.SetActive(false);
    }
}