using UnityEngine;

public class ButtonPressText : MonoBehaviour
{
    [SerializeField] private RectTransform buttonTextRect;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = buttonTextRect.localPosition;
    }

    public void OnButtonPress()
    {
        buttonTextRect.localPosition = originalPosition + new Vector3(0, 4f, 0);
    }

    public void OnButtonRelease()
    {
        buttonTextRect.localPosition = originalPosition;
    }
}
