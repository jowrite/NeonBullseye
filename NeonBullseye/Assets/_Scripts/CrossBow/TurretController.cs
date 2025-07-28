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
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private bool showTrajectory = true; // Toggle to show/hide trajectory line
    [SerializeField] private int trajectorySteps = 30; // Number of steps in the trajectory line
    [SerializeField] private float timeStep = 0.1f; //Time between points

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
            
            if (showTrajectory)
            {
                DrawTrajectory(currentCharge); // Draw the trajectory line while charging
            }
            //Add visual feedback for charging here
        }
    }

    
    #endregion

    #region Movement and Aiming
    private void HandleVerticalMovement(float inputValue)
    {
        if (GameManager.gm.isGamePaused || !GameManager.gm.isGameStarted) return;

     
        Vector2 move = rb.position + Vector2.up * inputValue * verticalSpeed * Time.deltaTime; // Move the turret upwards
        // Clamp the Y position within bounds
        move.y = Mathf.Clamp(move.y, minY, maxY);
        // Apply the new position to the turret
        rb.MovePosition(move);


    }

    private void HandleRotation(float inputValue)
    {
        if (GameManager.gm.isGamePaused || !GameManager.gm.isGameStarted) return;


        //Convert world rotation to -180 to 180 range
        float currentZ = transform.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f; // Normalize to -180 to 180 range

        //Apply input
        float newZ = currentZ + inputValue * rotationSpeed * Time.deltaTime;
        newZ = Mathf.Clamp(newZ, minAngle, maxAngle);

        transform.localRotation = Quaternion.Euler(0f, 0f, newZ); // Set the new rotation of the turret


    }

    private void StartCharging()
    {
        if (GameManager.gm.isGamePaused) return;
        isCharging = true;
        currentCharge = 0f; // Reset charge power when starting to charge
    }

    private void DrawTrajectory(float currentCharge)
    {
        Vector3[] points = new Vector3[trajectorySteps];
        Vector2 start = arrowSpawnPoint.position;

        float angleDeg = -transform.localEulerAngles.z; // Get the current turret angle in degrees, negative because Unity's coordinate system has 0 degrees pointing right
        float angleRad = angleDeg * Mathf.Deg2Rad; // Convert to radians

        float speed = currentCharge * 0.1f; // Scale speed based on charge power
        Vector2 initialVelocity = new Vector2(speed * Mathf.Cos(angleRad), speed * Mathf.Sin(angleRad));

        //Use positive gravity value but subtract in calculations
        float gravityValue = gravityScale * Mathf.Abs(Physics2D.gravity.y);

        for (int i = 0; i < trajectorySteps; i++)
        {
            float t = i * timeStep; // Time increment for each step
            points[i] = new Vector3(
                start.x + initialVelocity.x * t, 
                start.y + initialVelocity.y * t - 0.5f * gravityValue * t * t, 0f);
           // Calculate the position at time t using kinematic equations
        }

        trajectoryLine.positionCount = trajectorySteps;
        trajectoryLine.SetPositions(points); // Set the positions of the trajectory line renderer
    }

    private void ReleaseArrow()
    {
        if (!isCharging || GameManager.gm.isGamePaused) return;

        isCharging = false;
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, transform.rotation);
        
        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        
        float turretAngle = -transform.localEulerAngles.z; // Get the current turret angle (negative here too)
        arrowController.Launch(currentCharge, gravityScale, turretAngle); // Launch the arrow with the current charge power and angle

        currentCharge = 0f; // Reset charge power after shooting
        GameManager.gm.ArrowShot(); //Notify GameManager
        trajectoryLine.positionCount = 0; // Clear the trajectory line after shooting
    }

    #endregion
}
