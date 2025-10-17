using System.Collections;
using UnityEngine;

/// <summary>
/// 버튼 페이드인 완료 후(또는 지정 지연 시간 후) Wood 오브젝트를 부드럽게 페이드인시키는 스크립트.
/// - Wood 오브젝트에는 CanvasGroup이 필요합니다(자동으로 추가 가능).
/// - waitForGroup을 지정하면, 해당 그룹의 페이드인이 끝난 뒤에 시작합니다.
/// </summary>
[DisallowMultipleComponent]
public class WoodAnimator : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("페이드인시킬 대상(보통 Wood 오브젝트의 CanvasGroup)")]
    [SerializeField] private CanvasGroup woodGroup;

    [Tooltip("이 그룹의 페이드인(알파≈1) 이후에 시작하고 싶다면 할당 (예: ButtonsCanvasGroup)")]
    [SerializeField] private CanvasGroup waitForGroup;

    [Header("Timing")]
    [Tooltip("waitForGroup 완료 후 추가로 기다릴 시간(초)")]
    [SerializeField] private float startDelay = 0.2f;

    [Tooltip("Wood 페이드인에 걸리는 시간(초)")]
    [SerializeField] private float fadeDuration = 2.0f;

    [Header("Easing / Motion (옵션)")]
    [Tooltip("알파 보간 커브(미지정 시 Linear)")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("페이드인과 함께 약간의 스케일 인 효과를 줄지 여부")]
    [SerializeField] private bool useScaleIn = true;

    [Tooltip("시작 스케일(1이면 스케일 변화 없음)")]
    [SerializeField] private float startScale = 0.95f;

    [Tooltip("플레이 시작 시 자동 재생")]
    [SerializeField] private bool playOnStart = true;

    Coroutine _co;

    private void Reset()
    {
        // 에디터에서 컴포넌트 추가 시 자동 세팅
        if (!woodGroup) woodGroup = GetComponent<CanvasGroup>();
        if (!woodGroup) woodGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (!woodGroup)
            woodGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        // 초기 상태: 보이지 않게
        woodGroup.alpha = 0f;

        if (useScaleIn)
        {
            var t = transform as RectTransform;
            if (t != null) t.localScale = Vector3.one * startScale;
            else transform.localScale = Vector3.one * startScale;
        }
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    /// <summary>
    /// 외부에서 호출 가능. (예: 메뉴 초기화 끝난 뒤 수동 트리거)
    /// </summary>
    public void Play()
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        // 1) waitForGroup이 있으면 완료(알파≈1)까지 대기
        if (waitForGroup != null)
        {
            // 알파가 1에 근접할 때까지 대기
            while (waitForGroup.alpha < 0.999f)
                yield return null;
        }

        // 2) 추가 지연
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // 3) 페이드 인 (알파 + 선택적 스케일 인)
        float t = 0f;
        float dur = Mathf.Max(0.0001f, fadeDuration);
        float s0 = useScaleIn ? startScale : 1f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            float k = ease != null ? ease.Evaluate(p) : p;

            woodGroup.alpha = Mathf.Lerp(0f, 1f, k);

            if (useScaleIn)
            {
                float s = Mathf.Lerp(s0, 1f, k);
                var rt = transform as RectTransform;
                if (rt != null) rt.localScale = Vector3.one * s;
                else transform.localScale = Vector3.one * s;
            }

            yield return null;
        }

        woodGroup.alpha = 1f;
        if (useScaleIn)
        {
            var rt = transform as RectTransform;
            if (rt != null) rt.localScale = Vector3.one;
            else transform.localScale = Vector3.one;
        }

        _co = null;
    }
}
