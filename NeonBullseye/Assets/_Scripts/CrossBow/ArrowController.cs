using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class ArrowController : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float initialSpeed = 10f; // Initial speed of the arrow
    [SerializeField] private float maxLifetime = 10f; // Maximum lifetime before arrow is destroyed
    [SerializeField] private float launchAngle = 45f; // Launch angle in degrees
    [SerializeField] private float gravity = 9.8f;

    [SerializeField] private Vector3 startPosition; // Initial position of the arrow, assign in Inspector (tip of crossbow)
    private float launchTime;
    private float horizontalVel;
    private float verticalVel;

    private Rigidbody2D rb;
    private bool isLaunched;
    private bool isStuck;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Set to be Kinematic to control movement manually
        startPosition = transform.position; // Store the initial position of the arrow

    }

    private void FixedUpdate()
    {
        if (!isLaunched || isStuck) return;

        float timeSinceLaunch = Time.time - launchTime;

        //Manually calculate the position
        float x = startPosition.x + horizontalVel * timeSinceLaunch;
        float y = startPosition.y + verticalVel * timeSinceLaunch - 0.5f * gravity * timeSinceLaunch * timeSinceLaunch;

        transform.position = new Vector2(x, y);

        //Rotate to face the velocity direction
        float currentVy = verticalVel - gravity * timeSinceLaunch;
        Vector2 velocity = new Vector2(horizontalVel, currentVy);
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        //Destroy arrow after max lifetime
        if (timeSinceLaunch > maxLifetime)
        {
            Destroy(gameObject);
        }


    }

    public void Launch(float speed, float gravityStrength, float angleDegrees)
    {
        isLaunched = true;
        launchTime = Time.time;
        startPosition = transform.position;

        initialSpeed = speed;
        gravity = gravityStrength;
        launchAngle = angleDegrees;

        float angleRad = launchAngle * Mathf.Deg2Rad; // Convert angle to radians
        horizontalVel = initialSpeed * Mathf.Cos(angleRad); // Calculate horizontal velocity
        verticalVel = initialSpeed * Mathf.Sin(angleRad); // Calculate vertical velocity
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
        
        rb.linearVelocity = Vector2.zero; // Stop any remaining movement
    }

    
}

