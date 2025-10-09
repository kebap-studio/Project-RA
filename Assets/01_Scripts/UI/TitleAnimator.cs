using UnityEngine;
using TMPro; 
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TitleAnimator : MonoBehaviour
{
    [Header("Title Settings")]
    [SerializeField] private string titleText = "PROJECT-RA";
    [SerializeField] private float delayBeforeStart = 0.5f;
    
    [Header("Animation Type")]
    [SerializeField] private AnimationType animationType = AnimationType.TypeWriter;
    
    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.1f;
    [SerializeField] private bool showCursor = true;
    [SerializeField] private string cursorChar = "|";
    [SerializeField] private float cursorBlinkRate = 0.5f;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 2f;
    
    [Header("Scale Settings")]
    [SerializeField] private float scaleAnimationDuration = 1.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private TextMeshProUGUI titleTextComponent;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Coroutine blinkCoroutine;

    public enum AnimationType
    {
        TypeWriter,
        FadeIn,
        ScaleUp,
        FadeAndScale
    }

    private void Awake()
    {
        titleTextComponent = GetComponent<TextMeshProUGUI>(); 
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        originalScale = transform.localScale;
    }
    
    private void Start()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(AnimateTitle());
    }
    
    private void OnDestroy()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
    }

    private IEnumerator AnimateTitle()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        switch (animationType)
        {
            case AnimationType.TypeWriter:
                yield return StartCoroutine(TypeWriterAnimation());
                break;
            case AnimationType.FadeIn:
                yield return StartCoroutine(FadeInAnimation());
                break;
            case AnimationType.ScaleUp:
                yield return StartCoroutine(ScaleUpAnimation());
                break;
            case AnimationType.FadeAndScale:
                yield return StartCoroutine(FadeAndScaleAnimation());
                break;
        }
    }

    private IEnumerator TypeWriterAnimation()
    {
        titleTextComponent.text = "";
        canvasGroup.alpha = 1f;

        // 타이핑 애니메이션
        for (int i = 0; i <= titleText.Length; i++)
        {
            titleTextComponent.text = titleText.Substring(0, i);
            if (showCursor && i < titleText.Length)
                titleTextComponent.text += cursorChar;
            
            yield return new WaitForSeconds(typingSpeed);
        }

        // 커서 깜빡임
        if (showCursor)
        {
            StartCoroutine(BlinkCursor());
        }
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            titleTextComponent.text = titleText + cursorChar;
            yield return new WaitForSeconds(cursorBlinkRate);
            
            titleTextComponent.text = titleText;
            yield return new WaitForSeconds(cursorBlinkRate);
        }
    }

    private IEnumerator FadeInAnimation()
    {
        titleTextComponent.text = titleText;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator ScaleUpAnimation()
    {
        titleTextComponent.text = titleText;
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleAnimationDuration;
            float curveValue = scaleCurve.Evaluate(progress);
            
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, curveValue);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator FadeAndScaleAnimation()
    {
        titleTextComponent.text = titleText;
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, scaleCurve.Evaluate(progress));
            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = originalScale;
    }
}
