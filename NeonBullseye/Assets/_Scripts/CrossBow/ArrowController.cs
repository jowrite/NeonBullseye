using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class ArrowController : MonoBehaviour
{
    [Header("Physics Settings")]
    
    [SerializeField] private float maxLifetime = 10f; // Maximum lifetime before arrow is destroyed
    
    
    private Rigidbody2D rb;
    private bool isLaunched;
    private bool isStuck;
    private Vector2 startPosition; // Initial position of the arrow, assign in Inspector (tip of crossbow)
    private float launchTime;
    private Vector2 initialVelocity; // Initial speed of the arrow
    private float gravity;

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

        //Manually calculate the position using kinematic equations
        Vector2 currentPosition = new Vector2(
            startPosition.x + initialVelocity.x * timeSinceLaunch,
            startPosition.y + initialVelocity.y * timeSinceLaunch - 0.5f * gravity * timeSinceLaunch * timeSinceLaunch
        );

        //Rotate to face the velocity direction
        Vector2 currentVelocity = new Vector2(initialVelocity.x, initialVelocity.y - gravity * timeSinceLaunch);

        //Update position and rotation
        rb.MovePosition(currentPosition);
        float currentAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg; // Convert to degrees
        transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);

        //Destroy arrow after max lifetime
        if (timeSinceLaunch > maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(float speed, float gravityScale, float angleDegrees)
    {
        isLaunched = true;
        launchTime = Time.time;
        startPosition = transform.position;
        gravity = gravityScale * Mathf.Abs(Physics2D.gravity.y); //Convert scale to actual gravity value

        //Convert angle to radians (accounting Unity's coordinate system ***0 degrees is right, 90 degrees is up)
        float angleRad = (90f - angleDegrees) * Mathf.Deg2Rad;

        //Calculate initial velocity components
        initialVelocity = new Vector2(
            speed * Mathf.Cos(angleRad), 
            speed * Mathf.Sin(angleRad)
        );

        //Debug to verify values
        Debug.Log($"Launching arrow with speed={speed}, angle={angleDegrees}, velocity={initialVelocity}, gravity={gravity}");

        //Set initial rotation
        float launchAngle = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * Mathf.Rad2Deg; // Convert to degrees
        transform.rotation = Quaternion.AngleAxis(launchAngle, Vector3.forward); // Set the rotation of the arrow

        //Debugging visuals
        Debug.DrawRay(transform.position, initialVelocity.normalized * 2f, Color.magenta, 2f);

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

