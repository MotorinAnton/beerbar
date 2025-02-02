using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text buttonText;
    private Vector3 originalScale;
    public Color originalColor;
    public float scaleFactor = 1.2f;
    

    private void Start()
    {
        originalScale = buttonText.transform.localScale;
        originalColor = buttonText.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.transform.localScale = originalScale * scaleFactor;
        buttonText.color = Color.HSVToRGB(0.12f,0.81f,0.85f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.transform.localScale = originalScale;
        buttonText.color = originalColor;
    }
}