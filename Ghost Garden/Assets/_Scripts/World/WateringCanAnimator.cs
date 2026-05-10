using UnityEngine;
using System.Collections;

// Attach alongside Rigidbody on the watering can.
// Rigidbody start state: Is Kinematic = ON, Use Gravity = OFF

public class WateringCanAnimator : MonoBehaviour
{
    [Header("Fall Settings")]
    public Vector3 fallDirection = Vector3.forward;
    public float tipSpeed  = 120f;
    public float tipAngle  = 50f;

    Rigidbody _rb;
    bool _falling;
    bool _landed;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void FallOff()
    {
        if (_falling) return;
        _falling = true;

        // Play the shelf rattle / can sliding sound when nudged
        AudioManager.Instance?.PlayWateringCan(transform.position);

        StartCoroutine(TipThenFall());
    }

    IEnumerator TipThenFall()
    {
        float rotated  = 0f;
        Vector3 tipAxis = Vector3.Cross(Vector3.up, fallDirection.normalized);

        while (rotated < tipAngle)
        {
            float step = tipSpeed * Time.deltaTime;
            transform.Rotate(tipAxis, step, Space.World);
            rotated += step;
            yield return null;
        }

        _rb.isKinematic = false;
        _rb.useGravity   = true;
        _rb.linearVelocity    = fallDirection.normalized * 2f;
        _rb.angularVelocity   = tipAxis * 3f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!_falling || _landed) return;
        _landed = true;

        // Heavy thud when it hits the ground
        AudioManager.Instance?.PlayThud(transform.position);
    }
}