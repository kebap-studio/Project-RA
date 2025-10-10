using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [Header("Game Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Visual Effects")]
    [SerializeField] private CanvasGroup buttonsCanvasGroup;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;


    private bool _isTransitioning = false; // Start Button 클릭 시 애니메이션 동안 true

    private void Start()
    {
        if (buttonsCanvasGroup == null)
        {
            Debug.LogError("MenuCanvasGroup이 할당되지 않았습니다.");
            return;
        }

        SetupButtons();
        StartCoroutine(FadeInButtons()); // 메뉴가 부드럽게 나타나기
    }

    private void SetupButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGameClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitGameClicked);
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartGameClicked);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitGameClicked);

        // 코루틴 정리
        StopAllCoroutines();
    }

    private IEnumerator FadeInButtons()
    {
        // 버튼 비활성화 (애니메이션 중 클릭 방지)
        SetButtonsInteractable(false);

        // 초기 투명도 0
        buttonsCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            buttonsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        // 완료 후 버튼 활성화
        buttonsCanvasGroup.alpha = 1f;
        SetButtonsInteractable(true);
    }

    private IEnumerator FadeOutMenu(System.Action onComplete)
    {
        SetButtonsInteractable(false);

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            buttonsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        buttonsCanvasGroup.alpha = 0f;
        onComplete?.Invoke(); // 콜백 실행
    }

    private void OnExitGameClicked()
    {
        if (_isTransitioning) return;

        _isTransitioning = true;
        SetButtonsInteractable(false); // 모든 버튼 비활성화
        
        StartCoroutine(FadeOutMenu(() =>
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }));
    }

    private void OnStartGameClicked()
    {
        if (_isTransitioning) return;

        _isTransitioning = true;
        SetButtonsInteractable(false); // 모든 버튼 비활성화

        StartCoroutine(FadeOutMenu(() => { SceneLoader.LoadScene(gameSceneName); }));
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (startButton != null)
            startButton.interactable = interactable;
        if (exitButton != null)
            exitButton.interactable = interactable;
    }

    private void StartGame()
    {
        SceneLoader.LoadScene(gameSceneName);
    }

    private void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
