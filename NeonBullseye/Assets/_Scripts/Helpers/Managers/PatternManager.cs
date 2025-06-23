using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternManager : MonoBehaviour
{

    [Header("Pattern Settings")]
    [SerializeField] private float patternDisplayInterval = 1.5f;
    [SerializeField] private int initialPatternTargets = 3;
    [SerializeField] private int maxPatternTargets = 8;
    [SerializeField] private float postPatternDelay = 0.5f; //Delay buffer

    [Header("Debug")]
    [SerializeField] private bool allowHitsDuringDisplay = false; // Design decision, can be turned off

    private List<TargetFSM> allTargets = new List<TargetFSM>();
    private List<TargetFSM> currentPattern = new List<TargetFSM>();
    private List<TargetFSM> playerInputSequence = new List<TargetFSM>();
    private int currentPatternLength;
    private bool isDisplayingPattern;
    private bool needsTargetRefresh = true;

    private void Start()
    {
        CacheAllTargets();
    }

    private void CacheAllTargets()
    {
        allTargets.Clear();

        //Debug
        Debug.Log("Starting target search");

        TargetFSM[] targets = FindObjectsByType<TargetFSM>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        Debug.Log($"Found {targets.Length} targets in scene.");

        foreach (TargetFSM target in targets)
        {
            bool isActive = target.gameObject.activeInHierarchy;
            string sceneName = target.gameObject.scene.name;

            Debug.Log($"Target: {target.name} | " +
                     $"Active: {isActive} | " +
                     $"Scene: {sceneName} | " +
                     $"Has Collider: {target.GetComponent<Collider2D>() != null}");

            if (target.gameObject.scene.isLoaded)
            {
                allTargets.Add(target);
                Debug.Log($"Added {target.name} to available targets");
            }
        }
        //var targets = FindObjectsByType<TargetFSM>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        //foreach (var target in targets)
        //{
        //    if (target.gameObject.scene.isLoaded)
        //        allTargets.Add(target);
        //}

        if (allTargets.Count == 0)
        {
            Debug.LogError("No targets found in scene!");
            Debug.LogError("- Are targets active in hierarchy?");
            Debug.LogError("- Are they in a loaded scene?");
            Debug.LogError("- Do they have both TargetFSM and Collider2D components?");
        }
        else
        {
            Debug.Log($"Cached {allTargets.Count} targets successfully.");
        }
            needsTargetRefresh = false; // Disable refresh after initial cache
    }

    public void StartNewRound(int currentRound)
    {

        if (needsTargetRefresh) CacheAllTargets();
        
        currentPatternLength = Mathf.Min(
            initialPatternTargets + Mathf.FloorToInt(currentRound * 0.5f), 
            maxPatternTargets);
        
        GeneratePattern();
        StartCoroutine(PatternRoutine());
    }

    private void GeneratePattern()
    {
        currentPattern.Clear();
        playerInputSequence.Clear();

        //Ensure unique targets in pattern
        List<TargetFSM> availableTargets = new List<TargetFSM>(allTargets);

        for (int i = 0; i < currentPatternLength && availableTargets.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableTargets.Count);
            currentPattern.Add(availableTargets[randomIndex]);
            availableTargets.RemoveAt(randomIndex); //Remove to ensure uniqueness

        }
    }

    private IEnumerator PatternRoutine()
    {

        isDisplayingPattern = true;

        //Disable player input during pattern display
        //***DESIGN DECISION, maybe allow player to hit targets during pattern display?***
        if (!allowHitsDuringDisplay) 
        { 
            GameManager.gm.isGamePaused = true;
        }

        //Initial delay before pattern starts
        yield return new WaitForSeconds(1.5f);

        foreach (TargetFSM target in currentPattern)
        {
            target.SetAsPatternTarget();
            yield return new WaitForSeconds(patternDisplayInterval);

            if (!target.IsHit)
                target.ResetToIdle();

            yield return new WaitForSeconds(0.3f);
        }

        //Buffer after pattern
        yield return new WaitForSeconds(postPatternDelay);

        // Enable player input after pattern display
        if (!allowHitsDuringDisplay)
        {
            GameManager.gm.isGamePaused = false;
        }

        isDisplayingPattern = false;
    }

    public void RegisterPlayerHit(TargetFSM target)
    {

        if (isDisplayingPattern && !allowHitsDuringDisplay) return;

        playerInputSequence.Add(target);

        //Check if sequence matches the pattern
        int currentIndex = playerInputSequence.Count - 1;

        //Early exit if wrong target hit
        if (playerInputSequence[currentIndex] != currentPattern[currentIndex])
        {
            //Wrong sequence, reset pattern
            playerInputSequence.Clear();
            GameManager.gm.PatternFailed();
            return;
        }
        
        //Only award completion after last correct hit
        if (playerInputSequence.Count == currentPattern.Count)
        {
            //Correct sequence, complete pattern
            GameManager.gm.PatternCompleted();
            playerInputSequence.Clear();
        }
    }
}
