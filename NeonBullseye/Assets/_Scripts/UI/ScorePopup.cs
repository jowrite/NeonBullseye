using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeDuration = 1f;
    private float timeElapsed;

    private Color originalColor;


    public void Setup(int score)
    {
        popupText.text = $"+{score}";
        originalColor = popupText.color;
    }

    private void Update()
    {
        //Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        //Fade out
        timeElapsed += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timeElapsed /fadeDuration);
        popupText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        if (timeElapsed >= fadeDuration)
        {
            Destroy(gameObject); // Destroy the popup after fading out
        }

    }
}
