using NUnit.Framework;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;


public class UIControls : MonoBehaviour
{
    [Header("Text Displays")]
    [SerializeField] private TMPro.TextMeshProUGUI scoreValue;
    [SerializeField] private TMPro.TextMeshProUGUI roundNumber;
    [SerializeField] private TMPro.TextMeshProUGUI targetsHit;
    [SerializeField] private TMPro.TextMeshProUGUI arrowsLeft;
    [SerializeField] private TMPro.TextMeshProUGUI patternAttemptsLeft;


    [Header("Arrow Icons")]
    [SerializeField] private Transform arrowContainer; //Parent object for holding arrow icons
    [SerializeField] GameObject arrowIconPrefab; // Prefab for arrow icon
    private List<GameObject> arrowIcons = new List<GameObject>(); // List to hold arrow icons


    public void UpdateScore(int score)
    {
        scoreValue.text = score.ToString();
    }

    public void UpdateRound(int currentRound, int totalRounds)
    {
        roundNumber.text = $"Round: {currentRound + 1} / {totalRounds}";
    }

    public void UpdateTargetsHit(int hit, int required)
    {
        targetsHit.text = $"Targets: {hit} / {required}";
    }

    public void UpdateArrows(int current, int max)
    {
        arrowsLeft.text = $"Arrows Left: {current} / {max}";
        UpdateArrowIcons(current);
    }

    //Pattern attempts
    public void UpdatePatternAttempts(int remaining, int max)
    {
        patternAttemptsLeft.text = $"Pattern Attempts: {remaining} / {max}";
    }

    //Generate visual arrow icons each round
    public void SetupArrowIcons(int totalArrows)
    {
        //Clear old icons
        foreach (GameObject arrow in arrowIcons)
            Destroy(arrow);
        arrowIcons.Clear();

        //Instantiate new icons based on current arrows
        for (int i = 0; i < totalArrows; i++)
        {
            GameObject icon = Instantiate(arrowIconPrefab, arrowContainer);
            arrowIcons.Add(icon);
        }
    }

    private void UpdateArrowIcons(int arrowsRemaining)
    {
        for (int i = 0; i < arrowIcons.Count; i++)
        {
            arrowIcons[i].SetActive(i < arrowsRemaining);
        }
    }

    public void PatternSuccess()
    {
        //Handle visual feedback for successful pattern completion
        //This could be a popup, animation, or sound effect
        Debug.Log("Pattern completed successfully!");
    }

}
