using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float fieldOfView = 60f;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 20f, -16f);
    [SerializeField] private Transform target;

    [Header("Smooth Follow")]
    [SerializeField] private bool enableSmoothFollow = true;
    [SerializeField] private float smoothTime = 0.25f;

    private Camera _camera;
    private Vector3 _velocity = Vector3.zero; // SmoothDamp용 속도 변수

    private readonly Vector3 DEFAULT_POSITION = new Vector3(0f, 15f, -10f);
    private readonly Quaternion DEFAULT_ROTATION = Quaternion.Euler(45f, 0f, 0f);

    private void Awake()
    {
        InitializeCamera();
        FindTarget();
    }

    private void Start()
    {
        SetInitialCameraSettings();
        SetInitialPosition();
    }

    private void LateUpdate()
    {
        FollowTarget();
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

    private void SetInitialPosition()
    {
        Vector3 position = target != null ? target.position + offset : DEFAULT_POSITION;
        transform.position = position;
        transform.rotation = DEFAULT_ROTATION;
    }

    private void FollowTarget()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = enableSmoothFollow
            ? Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothTime)
            : desiredPosition;
    }

    // public
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
}
