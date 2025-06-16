using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gm;

    public bool isGamePaused = false;
    public bool isGameOver = false;
    public bool isGameStarted = false;

    public int currentRound = 0;
    //need values for current round, total rounds, and round time

    //need value for targets required to win the round

    //arrows/ammo count

    //pattern for the current round/array of targets to hit



    private void Awake()
    {
        if (gm == null) gm = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartRound();
    }

    private void StartRound()
    {
        Debug.Log("Starting Round: " + currentRound);
        isGameStarted = true;
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f; // Pauses the game
    }
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f; // Resumes the game
    }
    public void EndGame()
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pauses the game
        // Additional logic for ending the game (show game over UI & score)
    }

    

  

}
