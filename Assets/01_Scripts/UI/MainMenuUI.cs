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
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    private void Start()
    {
        SetupButtons();
        StartCoroutine(FadeInMenu()); // 메뉴가 부드럽게 나타나기
    }

    private void SetupButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGameClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitGameClicked);
    }

    private IEnumerator FadeInMenu()
    {
        // 버튼 비활성화 (애니메이션 중 클릭 방지)
        SetButtonsInteractable(false);

        // 초기 투명도 0
        menuCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        // 완료 후 버튼 활성화
        menuCanvasGroup.alpha = 1f;
        SetButtonsInteractable(true);
    }

    private IEnumerator FadeOutMenu(System.Action onComplete)
    {
        SetButtonsInteractable(false);

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        menuCanvasGroup.alpha = 0f;
        onComplete?.Invoke(); // 콜백 실행
    }

    private void OnExitGameClicked()
    {
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
