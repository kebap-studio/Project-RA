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

    private Vector3 _originalScale;
    private Color _originalColor;
    private Image _buttonImage;
    private Button _button;
    Coroutine _currentAnimation;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _buttonImage = GetComponent<Image>();

        _originalScale = transform.localScale;
        _originalColor = _buttonImage != null ? _buttonImage.color : normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        if (_currentAnimation != null) StopCoroutine((_currentAnimation));
        _currentAnimation = StartCoroutine(AnimateButton(_originalScale * hoverScale, hoverColor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        if (_currentAnimation != null) StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(AnimateButton(_originalScale, normalColor));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (_currentAnimation != null) StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(AnimateButton(_originalScale * clickScale, clickColor));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (_currentAnimation != null) StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(AnimateButton(_originalScale * hoverScale, hoverColor));
    }

    private IEnumerator AnimateButton(Vector3 targetScale, Color targetColor)
    {
        Vector3 startScale = transform.localScale;
        Color startColor = _buttonImage != null ? _buttonImage.color : normalColor;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;

            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);

            if (useColorAnimation && _buttonImage != null)
                _buttonImage.color = Color.Lerp(startColor, targetColor, progress);

            yield return null;
        }

        transform.localScale = targetScale;
        if (useColorAnimation && _buttonImage != null)
            _buttonImage.color = targetColor;
    }
}
