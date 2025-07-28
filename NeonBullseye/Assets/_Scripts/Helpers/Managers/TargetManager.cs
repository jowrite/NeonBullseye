using UnityEngine;

public class TargetManager : Singleton<TargetManager>
{
    [Header("Movement Settings")]
    [SerializeField] private float globalSwingSpeed = 30f;
    [SerializeField] private float globalMoveHeight = 1.5f;

    //Called when spawning/activating targets
    public void ConfigureTarget(GameObject target, TargetFSM.MovementType movementType)
    {
        TargetFSM targetFSM = target.GetComponent<TargetFSM>();

        //Override defaults if needed
        targetFSM.SetMovementParams(
            movementType, 
            globalSwingSpeed, 
            globalMoveHeight
        );
    }
}
