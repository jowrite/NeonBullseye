using System;
using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour
{
    
    [Header("Movement Settings")]
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;


    [Header("Aiming Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float minAngle = -60f;
    [SerializeField] private float maxAngle = 60f;

    [Header("Shooting Settings")]
    [SerializeField] private float maxChargePower = 50f;
    [SerializeField] private float chargeRate = 15f;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float gravityScale = 0.5f;

    private float currentCharge = 0f;
    private bool isCharging = false;
    private Rigidbody2D rb; // Reference to the Rigidbody2D component for physics interactions

    #region Setup and Initialization
    //Subscribe to input events
    private void Start()
    {
        //Using rigidbody for kinematic movement
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Set Rigidbody2D to kinematic for manual control
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        InputManager.Instance.OnVerticalMovement += HandleVerticalMovement;
        InputManager.Instance.OnRotation += HandleRotation;
        InputManager.Instance.OnChargeStart += StartCharging;
        InputManager.Instance.OnChargeRelease += ReleaseArrow;
    }

    private void OnDestroy()
    {
        //Unsubscribe from input events to avoid memory leaks
        InputManager.Instance.OnVerticalMovement -= HandleVerticalMovement;
        InputManager.Instance.OnRotation -= HandleRotation;
        InputManager.Instance.OnChargeStart -= StartCharging;
        InputManager.Instance.OnChargeRelease -= ReleaseArrow;
    }

    private void Update()
    {
        if (GameManager.gm.isGamePaused || !GameManager.gm.isGameStarted) return;

        if (isCharging)
        {
            currentCharge += chargeRate * Time.deltaTime; // Increment charge power while charging
            currentCharge = Mathf.Clamp(currentCharge, 0, maxChargePower); // Clamp the charge power to the maximum limit
            //Add visual feedback for charging here
        }
    }
    #endregion

    #region Movement and Aiming
    private void HandleVerticalMovement(float inputValue)
    {
        if (GameManager.gm.isGamePaused || !GameManager.gm.isGameStarted) return;

        // Calculate new position using 
        Vector2 newPosition = rb.position + Vector2.up * inputValue * verticalSpeed * Time.deltaTime; // Move the turret vertically
        // Clamp the Y position within bounds
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        // Apply the new position to the turret
        transform.position = newPosition;
        rb.MovePosition(newPosition);
    }

    private void HandleRotation(float inputValue)
    {
        if (GameManager.gm.isGamePaused || !GameManager.gm.isGameStarted) return;

        //T

        //Calculate rotation in local space
        float currentRotation = transform.localEulerAngles.z;

        // Normalize rotation to -180 to 180 range
        if (currentRotation > 180) currentRotation -= 360;
        // Clamp the rotation within bounds
        float newRotation = currentRotation - inputValue * rotationSpeed * Time.deltaTime;
        newRotation = Mathf.Clamp(newRotation, minAngle, maxAngle);
        // Apply the new rotation to the turret
        transform.localRotation = Quaternion.Euler(0, 0, newRotation); 
    }

    private void StartCharging()
    {
        if (GameManager.gm.isGamePaused) return;
        isCharging = true;
        currentCharge = 0f; // Reset charge power when starting to charge
    }

    private void ReleaseArrow()
    {
        if (!isCharging || GameManager.gm.isGamePaused) return;

        isCharging = false;
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, transform.rotation);
        
        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        
        float turretAngle = transform.localEulerAngles.z; // Get the current turret angle
        if (turretAngle > 180f) turretAngle -= 360f; // Normalize angle to -180 to 180 range
        arrowController.Launch(currentCharge, gravityScale, turretAngle); // Launch the arrow with the current charge power and angle

        currentCharge = 0f; // Reset charge power after shooting
        GameManager.gm.ArrowShot(); //Notify GameManager
    }

    #endregion
}
