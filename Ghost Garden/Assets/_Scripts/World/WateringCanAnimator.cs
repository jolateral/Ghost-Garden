using UnityEngine;
using System.Collections;

// Attach alongside Rigidbody on the watering can.
// Rigidbody start state: Is Kinematic = ON, Use Gravity = OFF

public class WateringCanAnimator : MonoBehaviour
{
    [Header("Fall Settings")]
    public Vector3 fallDirection = Vector3.forward; // point away from shelf toward neighbour path
    public float tipSpeed  = 120f;  // degrees per second during tip phase
    public float tipAngle  = 50f;   // degrees to rotate before physics takes over

    Rigidbody _rb;
    bool _falling;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void FallOff()
    {
        if (_falling) return;
        _falling = true;

        // Play FMOD watering can sound
        AudioManager.Instance?.PlayWateringCan(transform.position);

        StartCoroutine(TipThenFall());
    }

    IEnumerator TipThenFall()
    {
        // Phase 1: tip the can off the edge
        float rotated  = 0f;
        Vector3 tipAxis = Vector3.Cross(Vector3.up, fallDirection.normalized);

        while (rotated < tipAngle)
        {
            float step = tipSpeed * Time.deltaTime;
            transform.Rotate(tipAxis, step, Space.World);
            rotated += step;
            yield return null;
        }

        // Phase 2: hand off to physics
        _rb.isKinematic = false;
        _rb.useGravity   = true;
        _rb.linearVelocity    = fallDirection.normalized * 2f;
        _rb.angularVelocity   = tipAxis * 3f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_falling) return;
        // Play impact sound on first ground contact
        AudioManager.Instance?.PlayWateringCan(transform.position);
        _falling = false; // only play once
    }
}