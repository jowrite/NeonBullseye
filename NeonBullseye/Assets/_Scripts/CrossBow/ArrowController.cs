using System;
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


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Start as kinematic until launched

    }

    private void Update()
    {
        if (!isLaunched) return;

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f) Destroy(gameObject); // Destroy arrow after its lifetime expires

        //Rotate arrow to follow trajectory
        if (rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
    }

    public void Launch(float power)
    {
        rb.bodyType = RigidbodyType2D.Dynamic; // Switch to dynamic for physics simulation
        rb.AddForce(transform.right * power * launchMultiplier, ForceMode2D.Impulse);
        isLaunched = true;
        lifetimeTimer = maxLifetime; // Reset lifetime timer
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Target"))
        {
            TargetFSM target = other.GetComponent<TargetFSM>();
            if (target != null) target.HandleHit(); // Call the target's hit handling method

            //Stick arrow to target
            transform.SetParent(other.transform);
            rb.linearVelocity = Vector2.zero; // Stop arrow movement
            enabled = false; // Disable arrow controller script
        }
        else if (other.CompareTag("Boundary"))
        {
            Destroy(gameObject); // Destroy arrow on missing target
        }
    }
}
