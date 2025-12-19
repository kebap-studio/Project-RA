using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    // 타이머 설정
    [SerializeField] private float initTime = 0f;
    [SerializeField] private float targetTime = 600f;

    private float _currentTime;
    private bool _isRunning = true;
    private int _lastDisplayedSeconds = -1; // 마지막 표시된 초

    private void Start()
    {
        _currentTime = initTime;
        UpdateDisplay();
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        _currentTime += Time.deltaTime;

        // 타겟 시간에 도달하면 종료
        if (_currentTime >= targetTime)
        {
            _currentTime = targetTime;
            _isRunning = false;
            UpdateDisplay();
            OnTimerEnd();
            return; 
        }

        // 초가 변했을 때만 화면 업데이트
        int currentSeconds = Mathf.FloorToInt(_currentTime);
        if (currentSeconds != _lastDisplayedSeconds)
        {
            UpdateDisplay();
            _lastDisplayedSeconds = currentSeconds;
        }
    }

    private void UpdateDisplay()
    {
        int minutes = Mathf.FloorToInt(_currentTime / 60f);
        int seconds = Mathf.FloorToInt(_currentTime % 60f);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void PauseTimer()
    {
        _isRunning = false;
    }

    public void ResumeTimer()
    {
        _isRunning = true;
    }

    public void ResetTimer()
    {
        _isRunning = false;
        _currentTime = initTime;
        _lastDisplayedSeconds = -1;
        UpdateDisplay();
    }

    public void SetTime(float time)
    {
        _currentTime = Mathf.Clamp(time, initTime, targetTime);
        _lastDisplayedSeconds = -1;
        UpdateDisplay();
    }

    private void OnTimerEnd()
    {
        Debug.Log("Time's up!");
    }

    public float GetCurrentTime()
    {
        return _currentTime;
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public float GetProgress()
    {
        return Mathf.Clamp01(_currentTime / targetTime);
    }
}
