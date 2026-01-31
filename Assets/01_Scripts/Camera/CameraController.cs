using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float fieldOfView = 60f;

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // 캐릭터 어깨 높이

    [Header("Orbit Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 12f;

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Smooth Settings")]
    [SerializeField] private float positionSmoothTime = 0.1f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    [Header("Cursor Settings")]
    [SerializeField] private bool lockCursor = true;

    private Camera _camera;
    private float _yaw;   // 수평 회전
    private float _pitch; // 수직 회전
    private Vector3 _currentVelocity;

    private void Awake()
    {
        InitializeCamera();
        FindTarget();
    }

    private void Start()
    {
        SetInitialCameraSettings();
        InitializeCursorState();
        InitializeRotation();
    }

    private void Update()
    {
        // 클릭 시 커서 잠금 유지
        if (lockCursor && Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // ESC로 커서 잠금 해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleMouseInput();
        HandleScrollInput();
        UpdateCameraPosition();
    }

    private void InitializeCamera()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("Camera component not found on " + gameObject.name);
        }
    }

    private void FindTarget()
    {
        if (target != null) return;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "Player object not found. Please assign target manually or add 'Player' tag to the player object.");
        }
    }

    private void SetInitialCameraSettings()
    {
        if (_camera != null)
        {
            _camera.fieldOfView = fieldOfView;
        }
    }

    private void InitializeCursorState()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void InitializeRotation()
    {
        // 현재 카메라 회전을 기준으로 초기화
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;

        // pitch 값 보정 (Unity는 0~360으로 반환하므로)
        if (_pitch > 180f) _pitch -= 360f;
    }

    private void HandleMouseInput()
    {
        // 커서가 잠겨있지 않으면 마우스 입력 무시
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _yaw += mouseX;
        _pitch -= mouseY;

        // 수직 회전 제한
        _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * 5f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void UpdateCameraPosition()
    {
        // 타겟 위치 (어깨 높이 오프셋 적용)
        Vector3 targetPosition = target.position + targetOffset;

        // 회전 계산
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        // 카메라 위치 계산 (타겟 뒤쪽으로 distance만큼)
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // 부드러운 위치 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, positionSmoothTime);

        // 부드러운 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSmoothSpeed);
    }

    #region Public Methods

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    /// <summary>
    /// 커서 잠금 상태를 토글합니다
    /// </summary>
    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// 카메라의 수평 회전값(Yaw)을 반환합니다 - 플레이어 이동에 사용
    /// </summary>
    public float GetYaw()
    {
        return _yaw;
    }

    /// <summary>
    /// 카메라의 수평 방향 벡터를 반환합니다 (Y축 회전만 적용)
    /// </summary>
    public Quaternion GetHorizontalRotation()
    {
        return Quaternion.Euler(0f, _yaw, 0f);
    }

    #endregion
}
