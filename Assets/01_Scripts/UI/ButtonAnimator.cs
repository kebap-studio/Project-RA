using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem.Composites;

[RequireComponent(typeof(Button))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float animationDuration = 0.2f;

    [Header("Color Settings")]
    [SerializeField] private bool useColorAnimation = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.7f, 1f);
    [SerializeField] private Color clickColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    private Vector3 originalScale;
    private Color originalColor;
    private Image buttonImage;
    private Button button;
    Coroutine currentAnimation;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        originalScale = transform.localScale;
        originalColor = buttonImage != null ? buttonImage.color : normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (currentAnimation != null) StopCoroutine((currentAnimation));
        currentAnimation = StartCoroutine(AnimateButton(originalScale * hoverScale, hoverColor));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateButton(originalScale, normalColor));
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateButton(originalScale * clickScale, clickColor));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateButton(originalScale * hoverScale, hoverColor));
    }

    private IEnumerator AnimateButton(Vector3 targetScale, Color targetColor)
    {
        Vector3 startScale = transform.localScale;
        Color startColor = buttonImage != null ? buttonImage.color : normalColor;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;

            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);

            if (useColorAnimation && buttonImage != null)
                buttonImage.color = Color.Lerp(startColor, targetColor, progress);
            
            yield return null;
        }
        
        transform.localScale = targetScale;
        if (useColorAnimation && buttonImage != null)
            buttonImage.color = targetColor;
    }

}
