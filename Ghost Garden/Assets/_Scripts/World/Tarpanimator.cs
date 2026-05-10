using UnityEngine;
using System.Collections;

// Attach this to your tarp GameObject (which must have a MeshFilter + MeshRenderer).
// The tarp will fold up at its centre, rise with the birds, then disappear.
//
// SETUP IN INSPECTOR:
//   - Assign the Birds array (same birds as BirdController)
//   - The tarp mesh should be a plane subdivided at least 4x4
//     (in Unity: create a Plane, it's already subdivided enough)

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TarpAnimator : MonoBehaviour
{
    [Header("References")]
    public Transform[] birds; // assign same birds as BirdController

    [Header("Fold Settings")]
    public float foldDuration   = 1.2f;  // seconds to fold up
    public float raiseDuration  = 2.5f;  // seconds to fly upward with birds
    public float raiseHeight    = 8f;    // how high it goes before disappearing

    [Header("Fold Shape")]
    // How much each vertex droops downward at the edges when folding (gives cloth look)
    public float edgeDroop = 0.4f;
    public AnimationCurve foldCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Mesh _mesh;
    Vector3[] _originalVerts;
    Vector3[] _currentVerts;
    bool _animating;

    void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _originalVerts = _mesh.vertices;
        _currentVerts  = (Vector3[])_originalVerts.Clone();
    }

    // Called by BirdController when birds are scared away
    public void BeginCarryAway()
    {
        if (_animating) return;
        StartCoroutine(CarryAwayRoutine());
    }

    IEnumerator CarryAwayRoutine()
    {
        _animating = true;

        // ── Phase 1: Fold the tarp up (vertices pull toward centre) ──────────
        float elapsed = 0f;
        Bounds bounds = _mesh.bounds;
        Vector3 centre = bounds.center;
        float maxDist   = Mathf.Max(bounds.extents.x, bounds.extents.z);

        while (elapsed < foldDuration)
        {
            float t = foldCurve.Evaluate(elapsed / foldDuration);

            for (int i = 0; i < _currentVerts.Length; i++)
            {
                Vector3 orig = _originalVerts[i];

                // Horizontal pull toward centre
                float distFromCentre = Vector2.Distance(
                    new Vector2(orig.x, orig.z),
                    new Vector2(centre.x, centre.z));
                float normDist = distFromCentre / maxDist; // 0=centre, 1=edge

                // Pull edges inward
                Vector3 toCentre = new Vector3(
                    centre.x - orig.x, 0f, centre.z - orig.z).normalized;
                Vector3 pulled = orig + toCentre * normDist * t * (maxDist * 0.9f);

                // Droop edges downward (gives cloth fold look)
                pulled.y = orig.y - edgeDroop * normDist * t;

                _currentVerts[i] = pulled;
            }

            _mesh.vertices = _currentVerts;
            _mesh.RecalculateNormals();
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ── Phase 2: Rise upward with the birds ──────────────────────────────
        elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * raiseHeight;

        // Attach tarp to follow the centre-point of the birds
        while (elapsed < raiseDuration)
        {
            float t = elapsed / raiseDuration;

            // Move tarp upward
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            // Slightly wobble the mesh as it flies (cloth flutter)
            for (int i = 0; i < _currentVerts.Length; i++)
            {
                float wave = Mathf.Sin(Time.time * 6f + i * 0.4f) * 0.05f * (1f - t);
                _currentVerts[i].y += wave * Time.deltaTime * 10f;
            }
            _mesh.vertices = _currentVerts;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ── Phase 3: Fade out and destroy ────────────────────────────────────
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material mat    = mr.material;

        // Make sure the material supports transparency
        // (if your tarp material is Opaque, set it to Transparent in Inspector)
        Color col = mat.color;
        float fadeTime = 0.6f;
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            col.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            mat.color = col;
            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}