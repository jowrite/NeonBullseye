using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternManager : MonoBehaviour
{

    [Header("Pattern Settings")]
    [SerializeField] private float patternDisplayInterval = 3f;
    [SerializeField] private int initialPatternTargets = 3;
    [SerializeField] private int maxPatternTargets = 8;

    private List<TargetFSM> allTargets = new List<TargetFSM>();
    private List<TargetFSM> currentPattern = new List<TargetFSM>();
    private List<TargetFSM> playerInputSequence = new List<TargetFSM>();
    private int currentPatternLength;

    private void Awake()
    {
        // Find all targets in the scene
        TargetFSM[] targets = Object.FindObjectsByType<TargetFSM>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        allTargets = new List<TargetFSM>(targets);
    }

    public void StartNewRound(int currentRound)
    {
        currentPatternLength = Mathf.Min(initialPatternTargets + currentRound, maxPatternTargets);
        GeneratePattern();
        StartCoroutine(DisplayPattern());
    }

    private void GeneratePattern()
    {
        currentPattern.Clear();
        playerInputSequence.Clear();

        for (int i = 0; i < currentPatternLength; i++)
        {
            int randomIndex = Random.Range(0, allTargets.Count);
            currentPattern.Add(allTargets[randomIndex]);
        }
    }

    private IEnumerator DisplayPattern()
    {
        //Disable player input during pattern display
        //***DESIGN DECISION, maybe allow player to hit targets during pattern display?***
        GameManager.gm.isGamePaused = true;

        foreach (TargetFSM target in currentPattern)
        {
            target.SetAsPatternTarget();
            yield return new WaitForSeconds(patternDisplayInterval);

            // Use a public or accessible method to reset the target state
            target.HandleHit();
            yield return new WaitForSeconds(0.3f);
        }

        // Enable player input after pattern display
        GameManager.gm.isGamePaused = false;
    }

    public void RegisterPlayerHit(TargetFSM target)
    {
        playerInputSequence.Add(target);

        //Check if sequence matches the pattern
        int currentIndex = playerInputSequence.Count - 1;
        if (playerInputSequence[currentIndex] != currentPattern[currentIndex])
        {
            //Wrong sequence, reset pattern
            playerInputSequence.Clear();
            GameManager.gm.PatternFailed();
        }
        else if (playerInputSequence.Count == currentPattern.Count)
        {
            //Correct sequence, complete pattern
            GameManager.gm.PatternCompleted();
            playerInputSequence.Clear();
        }
    }
}
