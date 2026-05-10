using UnityEngine;
using System.Collections;

public class WindchimeAnimator : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAngle    = 18f;
    public float swaySpeed    = 2.5f;
    public float swayDuration = 4f;

    // Each child gets a small random offset so they don't all sway in sync,
    // which looks much more natural for a windchime
    [Range(0f, 1f)]
    public float phaseRandomness = 0.4f;

    bool _swaying;
    Transform[] _children;

    void Start()
    {
        // Collect all direct children of this parent object
        _children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            _children[i] = transform.GetChild(i);

        if (_children.Length == 0)
            Debug.LogWarning("[WindchimeAnimator] No children found — attach this script to the parent empty GameObject that contains your chime pieces.");
    }

    public void Sway()
    {
        if (_swaying) return;
        AudioManager.Instance?.PlayWindchime(transform.position);
        StartCoroutine(SwayRoutine());
    }

    IEnumerator SwayRoutine()
    {
        _swaying = true;

        // Store each child's starting rotation and give it a random phase offset
        Quaternion[] startRots   = new Quaternion[_children.Length];
        float[]      phaseOffset = new float[_children.Length];

        for (int i = 0; i < _children.Length; i++)
        {
            startRots[i]   = _children[i].localRotation;
            phaseOffset[i] = Random.Range(-phaseRandomness, phaseRandomness);
        }

        float elapsed = 0f;

        while (elapsed < swayDuration)
        {
            float t         = elapsed / swayDuration;
            float amplitude = swayAngle * (1f - t); // fade out over time

            for (int i = 0; i < _children.Length; i++)
            {
                float angle = Mathf.Sin((elapsed + phaseOffset[i]) * swaySpeed * Mathf.PI)
                            * amplitude;
                _children[i].localRotation = startRots[i] * Quaternion.Euler(0f, 0f, angle);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore all children to their original rotations
        for (int i = 0; i < _children.Length; i++)
            _children[i].localRotation = startRots[i];

        _swaying = false;
    }
}