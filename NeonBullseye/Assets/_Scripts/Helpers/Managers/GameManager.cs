using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_WEBGL
//Prevent input lag in browser
Input.ResetInputAxes();
Input.simulationMode = InputSimulationMode.ForceUnity;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager gm;

    public bool isGamePaused = false;
    public bool isGameOver = false;
    public bool isGameStarted = false;

 
    [Header("Game Settings")]
    [SerializeField] private int arrowsPerRound = 20;
    [SerializeField] private int targetsPerRound = 10;
    [SerializeField] private int totalRounds = 5; // Set this to the desired number of rounds
    private int currentArrows;
    private int currentRound = 0;
    private int score = 0;
    private int baseScore = 100; // Base score for hitting a target

    [Header("Pattern Settings")]
    [SerializeField] private int patternBonus = 500;
    private int currentTargetsHit;
    private PatternManager pm;


    private void Awake()
    {
        if (gm == null) gm = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Input.Instance.OnPause += TogglePause;
        pm = GetComponent<PatternManager>();
        StartRound();
    }

    private void StartRound()
    {
        currentArrows = arrowsPerRound;
        currentTargetsHit = 0;
        pm.StartNewRound(currentRound);
        Debug.Log("Starting Round: " + currentRound);
        isGameStarted = true;
        //UI updates for arrows/targets
    }

    private void TogglePause()
    {
        if (isGameOver) return; // Prevent toggling pause if the game is over

        if (isGamePaused) ResumeGame();
        else PauseGame();
    }

    #region Pause and Resume Methods
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f; // Pauses the game
        //Show pause menu UI

    }
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f; // Resumes the game
        //Hide pause menu UI
    }
    public void EndGame()
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pauses the game
        // Additional logic for ending the game (show game over UI & score)
    }

    #endregion

    #region Gameplay Methods
    //Gameplay methods for handling arrows, patterns and targets
    public void ArrowShot()
    {
        currentArrows--;
        if (currentArrows <= 0) EndRound();
    }
    public void TargetHit()
    {
        currentTargetsHit++;
        if (currentTargetsHit >= targetsPerRound) EndRound();
    }

    public void HandlePatternHit(TargetFSM target)
    {
        pm.RegisterPlayerHit(target);
        AddScore(baseScore); //Base points still awarded for hitting targets
    }

    public void PatternCompleted()
    {
        AddScore(patternBonus);
        //Special effects or UI updates for completing a pattern
    }

    public void PatternFailed()
    {
        //Visual feedback for failed pattern
    }
    #endregion

    #region Scoring & Rounds
    //Scoring & Round Management
    private void EndRound()
    {
        currentRound++;
        if (currentRound > totalRounds) EndGame();
        else Invoke("StartRound", 5f); //Delay before starting next round
    }

    public void AddScore(int points)
    {
        score += points;
    }
    #endregion
}
