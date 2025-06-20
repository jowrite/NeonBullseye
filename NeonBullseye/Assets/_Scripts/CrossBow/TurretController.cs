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

    private float currentCharge = 0f;
    private bool isCharging = false;

    #region Setup and Initialization
    //Subscribe to input events
    private void Start()
    {
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

        // Calculate new position based on input
        Vector3 newPosition = transform.position + Vector3.up * inputValue * verticalSpeed * Time.deltaTime;
        // Clamp the Y position within bounds
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        // Apply the new position to the turret
        transform.position = newPosition; 
    }

    private void HandleRotation(float inputValue)
    {
        if (GameManager.gm.isGamePaused || !GameManager.gm.isGameStarted) return;

        float newRotation = transform.eulerAngles.z - inputValue * rotationSpeed * Time.deltaTime; // Calculate new rotation based on input

        // Normalize rotation to -180 to 180 range
        if (newRotation > 180) newRotation -= 360;
        // Clamp the rotation within bounds
        newRotation = Mathf.Clamp(newRotation, minAngle, maxAngle);
        // Apply the new rotation to the turret
        transform.rotation = Quaternion.Euler(0, 0, newRotation); 
    }

    //Redundant shooting method commented out for clarity
    //private void HandleShooting()
    //{
    //    if (InputManager.GetKeyDown(KeyCode.Space)) // Check if the space key is pressed to start charging
    //    {
    //        StartCharging();
    //    }

    //    else if (InputManager.GetKeyUp(KeyCode.Space)) // Check if the space key is released to shoot
    //    {
    //        ReleaseArrow();
    //    }

    //    if (isCharging)
    //    {
    //        // Increment charge power while charging
    //        currentCharge += chargeRate * Time.deltaTime;
    //        // Clamp the charge power to the maximum limit
    //        currentCharge = Mathf.Clamp(currentCharge, 0, maxChargePower);
    //        //visual feedback for charging can be added here, e.g. changing UI or turret appearance
    //    }
    //}

    private void StartCharging()
    {
        isCharging = true;
        currentCharge = 0f; // Reset charge power when starting to charge
    }

    private void ReleaseArrow()
    {
        isCharging = false;
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        arrow.GetComponent<ArrowController>().Launch(currentCharge); // Pass the charge power to the arrow controller
        currentCharge = 0f; // Reset charge power after shooting
    }

    #endregion
}
