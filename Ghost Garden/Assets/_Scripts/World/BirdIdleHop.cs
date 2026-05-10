using UnityEngine;
using System.Collections;

// Attach this to each individual bird GameObject.
// The bird will hop around randomly within a small radius of its start position.
// Hopping stops automatically when BirdController.ScareAway() is called.
//
// SETUP:
//   1. Attach this script to each bird GameObject
//   2. Tune the settings in the Inspector
//   3. No other wiring needed — it starts automatically on Play

public class BirdIdleHop : MonoBehaviour
{
    [Header("Hop Area")]
    // How far from the bird's starting position it can wander
    public float wanderRadius = 0.6f;

    [Header("Hop Timing")]
    // Seconds between hops
    public float minHopInterval = 0.8f;
    public float maxHopInterval = 2.5f;

    [Header("Hop Motion")]
    // How high the bird lifts off the ground mid-hop
    public float hopHeight      = 0.15f;
    // How long a single hop takes
    public float hopDuration    = 0.2f;
    // How much the bird rotates to face its hop direction
    public bool  faceHopDirection = true;

    [Header("Idle Bob")]
    // Gentle head-bob while standing still between hops
    public bool  idleBob        = true;
    public float bobAmount      = 0.02f;
    public float bobSpeed       = 3f;

    // Set this to true from BirdController when the birds are scared away
    [HideInInspector] public bool scared = false;

    Vector3 _startPosition;   // world position where the bird began
    Vector3 _groundPosition;  // current resting position on the ground
    bool    _hopping;

    void Start()
    {
        _startPosition  = transform.position;
        _groundPosition = transform.position;
        StartCoroutine(HopLoop());
    }

    void Update()
    {
        // Gentle idle bob while not hopping and not scared
        if (!_hopping && !scared && idleBob)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = _groundPosition + Vector3.up * bob;
        }
    }

    IEnumerator HopLoop()
    {
        while (!scared)
        {
            // Wait a random interval before the next hop
            float wait = Random.Range(minHopInterval, maxHopInterval);
            float elapsed = 0f;
            while (elapsed < wait && !scared)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (scared) yield break;

            // Pick a random spot within wanderRadius of the start position
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 target = _startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            yield return StartCoroutine(HopTo(target));
        }
    }

    IEnumerator HopTo(Vector3 target)
    {
        _hopping = true;

        Vector3 startPos = _groundPosition;

        // Rotate to face the hop direction before jumping
        if (faceHopDirection)
        {
            Vector3 dir = (target - startPos);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        float elapsed = 0f;
        while (elapsed < hopDuration)
        {
            if (scared) yield break;

            float t = elapsed / hopDuration;

            // Lerp horizontally from start to target
            Vector3 flatPos = Vector3.Lerp(startPos, target, t);

            // Add a parabolic arc for the hop height
            // sin(t * PI) gives 0 at start, peak at mid, 0 at end
            float arc = Mathf.Sin(t * Mathf.PI) * hopHeight;

            transform.position = flatPos + Vector3.up * arc;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to exact target on landing
        transform.position = target;
        _groundPosition    = target;
        _hopping           = false;

        // Small squish on landing to give weight
        StartCoroutine(LandingSquish());
    }

    IEnumerator LandingSquish()
    {
        Vector3 original = transform.localScale;
        Vector3 squished = new Vector3(original.x * 1.2f, original.y * 0.75f, original.z * 1.2f);

        float duration = 0.06f;
        float elapsed  = 0f;

        // Squish down
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(original, squished, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        // Spring back
        while (elapsed < duration * 1.5f)
        {
            float t = elapsed / (duration * 1.5f);
            transform.localScale = Vector3.Lerp(squished, original, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = original;
    }

    // Called by BirdController when the birds are scared away
    public void StopHopping()
    {
        scared = true;
        StopAllCoroutines();
    }
}