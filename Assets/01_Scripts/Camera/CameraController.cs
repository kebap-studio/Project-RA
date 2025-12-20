using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float fieldOfView = 75f; // FOV 증가

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -3.5f); // 낮고 가깝게
    [SerializeField] private Transform target;
    [SerializeField] private bool lockRotation = true;

    [Header("Rotation Settings")]
    [SerializeField] private float cameraAngle = 12f; // 더 낮은 각도

    [Header("Smooth Follow")]
    [SerializeField] private bool enableSmoothFollow = false;
    [SerializeField] private float smoothTime = 0.15f; // 더 빠른 추적

    private Camera _camera;
    private Vector3 _velocity = Vector3.zero;
    private Quaternion _fixedRotation;

    private readonly Vector3 DEFAULT_POSITION = new Vector3(0f, 15f, -10f);

    private void Awake()
    {
        InitializeCamera();
        FindTarget();
    }

    private void Start()
    {
        SetInitialCameraSettings();
        SetInitialRotation();
        SetInitialPosition();
    }

    private void LateUpdate()
    {
        FollowTarget();
        ApplyRotation();
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

    private void SetInitialRotation()
    {
        _fixedRotation = Quaternion.Euler(cameraAngle, 0f, 0f);
        transform.rotation = _fixedRotation;
    }

    private void SetInitialPosition()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
        else
        {
            transform.position = DEFAULT_POSITION;
            transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
        }
    }

    private void FollowTarget()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = enableSmoothFollow
            ? Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime)
            : desiredPosition;
    }

    private void ApplyRotation()
    {
        if (lockRotation)
        {
            transform.rotation = _fixedRotation;
        }
    }

    // Public Methods
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    public void SetSmoothFollow(bool enabled, float newSmoothTime = 0.25f)
    {
        enableSmoothFollow = enabled;
        smoothTime = newSmoothTime;
    }

    public void SetCameraAngle(float angle)
    {
        cameraAngle = angle;
        SetInitialRotation();
    }

    public void SetRotationLock(bool locked)
    {
        lockRotation = locked;
    }
}
