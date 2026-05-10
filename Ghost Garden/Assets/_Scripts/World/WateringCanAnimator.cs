using UnityEngine;
using System.Collections;

// Attach this to the watering can GameObject alongside its Rigidbody.
// The Rigidbody should start as: Use Gravity = OFF, Is Kinematic = ON
// This script handles the tipping/falling animation when nudged.

public class WateringCanAnimator : MonoBehaviour
{
    [Header("Fall Settings")]
    // Direction the can tips off the shelf (set in Inspector to point away from shelf)
    public Vector3 fallDirection = Vector3.forward;
    // How fast the can rotates while tipping before going into full physics
    public float tipSpeed = 120f;
    // Degrees to rotate before handing off to physics (simulates tipping off edge)
    public float tipAngle = 45f;
    // Sound to play when the can hits the ground
    public AudioClip landSound;

    Rigidbody _rb;
    AudioSource _audio;
    bool _falling;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _audio = GetComponent<AudioSource>();

        if (_rb == null)
        {
            Debug.LogError("[WateringCanAnimator] No Rigidbody found on watering can!");
        }
    }

    // Called by WinSequenceManager when the can is nudged
    public void FallOff()
    {
        if (_falling) return;
        _falling = true;
        StartCoroutine(TipThenFall());
    }

    IEnumerator TipThenFall()
    {
        // Phase 1: rotate the can to simulate it tipping off the shelf edge
        float rotated = 0f;
        Vector3 tipAxis = Vector3.Cross(Vector3.up, fallDirection.normalized);

        while (rotated < tipAngle)
        {
            float step = tipSpeed * Time.deltaTime;
            transform.Rotate(tipAxis, step, Space.World);
            rotated += step;
            yield return null;
        }

        // Phase 2: hand off to physics for the rest of the fall
        _rb.isKinematic = false;
        _rb.useGravity = true;

        // Give it momentum in the fall direction so it lands in the path
        _rb.linearVelocity = fallDirection.normalized * 2f;
        _rb.angularVelocity = tipAxis * 3f;

        // Phase 3: wait for it to land and play a sound
        yield return new WaitForSeconds(0.1f); // let it start moving
        yield return new WaitUntil(() => _rb.IsSleeping() || _rb.linearVelocity.magnitude < 0.1f);

        if (_audio != null && landSound != null)
            _audio.PlayOneShot(landSound);

        Debug.Log("[WateringCanAnimator] Can has landed.");
    }

    // Detect collision with ground to play sound immediately on impact
    void OnCollisionEnter(Collision collision)
    {
        if (!_falling) return;
        if (_audio != null && landSound != null)
        {
            _audio.PlayOneShot(landSound);
        }
    }
}