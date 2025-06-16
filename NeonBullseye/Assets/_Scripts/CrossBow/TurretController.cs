using System;
using UnityEngine;

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

    private void Update()
    {
        if (GameManager.gm.isGamePaused || !GameManager.gm.isGameStarted) return;

        HandleMovement();
        HandleRotation();
        HandleShooting();
    }

    private void HandleMovement()
    {
        throw new NotImplementedException();
    }

    private void HandleRotation()
    {
        throw new NotImplementedException();
    }

    private void HandleShooting()
    {
        throw new NotImplementedException();
    }

    private void StartCharging()
    {
        throw new NotImplementedException();
    }

    private void ReleaseArrow()
    {
        throw new NotImplementedException();
    }


}
