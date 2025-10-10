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

    private TextMeshProUGUI _titleTextComponent;
    private CanvasGroup _canvasGroup;
    private Vector3 _originalScale;
    private Coroutine _blinkCoroutine;

    public enum AnimationType
    {
        TypeWriter,
        FadeIn,
        ScaleUp,
        FadeAndScale
    }

    private void Awake()
    {
        _titleTextComponent = GetComponent<TextMeshProUGUI>(); 
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        _originalScale = transform.localScale;
        
        
        _titleTextComponent.text = titleText + (showCursor ? cursorChar : "");
        _titleTextComponent.maxVisibleCharacters = 0;
    }
    
    private void Start()
    {
        if (_blinkCoroutine != null)
            StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(AnimateTitle());
    }
    
    private void OnDestroy()
    {
        if (_blinkCoroutine != null)
            StopCoroutine(_blinkCoroutine);
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
        _canvasGroup.alpha = 1f;

        // 타이핑 애니메이션
        for (int i = 0; i <= titleText.Length; i++)
        {
            _titleTextComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 커서 깜빡임
        if (showCursor)
            StartCoroutine(BlinkCursor());
    }

    private IEnumerator BlinkCursor()
    {
        int textLength = titleText.Length;
        int fullLength = titleText.Length + (showCursor ? 1 : 0);
    
        while (true)
        {
            // 커서 숨기기: 텍스트만 보이게
            _titleTextComponent.maxVisibleCharacters = textLength;
            yield return new WaitForSeconds(cursorBlinkRate);
        
            // 커서 보이기: 텍스트 + 커서
            _titleTextComponent.maxVisibleCharacters = fullLength;
            yield return new WaitForSeconds(cursorBlinkRate);
        }
    }

    private IEnumerator FadeInAnimation()
    {
        _titleTextComponent.text = titleText;
        _canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
    }

    private IEnumerator ScaleUpAnimation()
    {
        _titleTextComponent.text = titleText;
        _canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleAnimationDuration;
            float curveValue = scaleCurve.Evaluate(progress);
            
            transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, curveValue);
            yield return null;
        }

        transform.localScale = _originalScale;
    }

    private IEnumerator FadeAndScaleAnimation()
    {
        _titleTextComponent.text = titleText;
        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, scaleCurve.Evaluate(progress));
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        transform.localScale = _originalScale;
    }
}
