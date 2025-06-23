using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class ArrowController : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float launchMultiplier = 10f;
    [SerializeField] private float maxLifetime = 5f; //adjust as needed

    private Rigidbody2D rb;
    private float lifetimeTimer;
    private bool isLaunched;
    private bool isStuck;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Start as kinematic until launched

    }

    private void Update()
    {
        if (!isLaunched || isStuck) return;

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        //Rotate arrow to follow trajectory
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

    }

    public void Launch(float power, float gravityScale)
    {
        rb.bodyType = RigidbodyType2D.Dynamic; // Switch to dynamic for physics simulation
        rb.gravityScale = gravityScale;
        rb.AddForce(transform.right * power * launchMultiplier, ForceMode2D.Impulse);
        isLaunched = true;
        lifetimeTimer = maxLifetime; // Reset lifetime timer
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLaunched || isStuck) return;

        if (other.CompareTag("Target"))
        {
            StickToTarget(other.transform);
            TargetFSM target = other.GetComponent<TargetFSM>();
            if (target != null) target.HandleHit(); // Call the target's hit handling method
        }
        else if (other.CompareTag("Boundary"))
        {
            Destroy(gameObject); // Destroy arrow on missing target
        }
    }

    private void StickToTarget(Transform target)
    {
        isStuck = true;

        //Parent to target and disable physics
        transform.SetParent(target);
        rb.bodyType = RigidbodyType2D.Kinematic; // Disable physics interactions
        rb.linearVelocity = Vector2.zero; // Stop any remaining movement


        //Rotation arrow perpendicular to target surface
        Vector3 hitNormal = (transform.position - target.position).normalized;
        float stickAngle = Mathf.Atan2(hitNormal.y, hitNormal.x) * Mathf.Rad2Deg;
        StartCoroutine(RotateToStickPosition(Quaternion.Euler(0, 0, stickAngle + 90f)));
    }

    private IEnumerator RotateToStickPosition(Quaternion targetRotation)
    {
        float duration = 0.3f;
        float elapsed = 0;

        Quaternion startRotation = transform.rotation;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }
    }
}

