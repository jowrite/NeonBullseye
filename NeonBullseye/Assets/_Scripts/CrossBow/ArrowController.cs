using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class ArrowController : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float initialSpeed = 10f; // Initial speed of the arrow
    [SerializeField] private float horizontalVel;
    [SerializeField] private float verticalVel;
    [SerializeField] private float maxLifetime = 10f; // Maximum lifetime before arrow is destroyed
    [SerializeField] private float maxHeight = 5f; // Maximum height the arrow can reach
    [SerializeField] private float launchAngle = 45f; // Launch angle in degrees

    private Rigidbody2D rb;
    private float lifetimeTimer;
    private bool isLaunched;
    private bool isStuck;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Set to be Kinematic to control movement manually

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
    }

    public void Launch()
    {
        // NEED TO FIX THIS - MUST BE KINEMATIC ALWAYS, can't use physics engine
        
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
        
        rb.linearVelocity = Vector2.zero; // Stop any remaining movement
    }

    
}

