using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotateSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Components
    private CharacterController _characterController;
    private Animator _animator;

    // input
    private Vector2 _moveInput;

    // Movement
    private Vector3 _moveDirection;
    private Vector3 _velocity;

    private bool _isMoving = false;
    private bool _wasMoving = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        if (_animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            Debug.Log("PlayerInput 연결됨!");
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdateAnimations();

        if (showDebugInfo)
        {
            DebugInfo();
        }
    }

    private void HandleMovement()
    {
        Vector2 normalizedInput = _moveInput.normalized;
        _moveDirection = new Vector3(normalizedInput.x, 0f, normalizedInput.y);

        float currentSpeed = moveSpeed;

        _velocity = _moveDirection * currentSpeed;
        _characterController.Move(_velocity * Time.deltaTime);

        if (_animator != null)
        {
            // TODO: Animator 설정
        }
    }

    private void HandleRotation()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    private void DebugInfo()
    {
        Debug.DrawRay(transform.position, _moveDirection * 2f, Color.red);
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.blue);
    }

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        _isMoving = IsMoving();

        if (_isMoving != _wasMoving)
        {
            _animator.SetBool("IsMoving", _isMoving);
            _wasMoving = _isMoving;

            if (showDebugInfo)
            {
                Debug.Log($"Movement State: {(_isMoving ? "Moving" : "Idle")}");
            }
        }
    }

    // Input System Events
    void OnMove(InputValue value) 
    {
        _moveInput = value.Get<Vector2>();

        if (showDebugInfo)
            Debug.Log($"Move Input: {_moveInput}");
    }

    // 외부에서 호출 가능한 메서드들
    public Vector3 GetVelocity() => _velocity;
    public bool IsMoving() => _velocity.magnitude > 0.1f;
}
