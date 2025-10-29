using Unity.Mathematics.Geometry;
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
    private float _acceleration = 1.5f;
    private float _deceleration = 5f;

    // Attack Move
    private Vector3 _lastPosition;
    private bool _isLastPosition = false;

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

        if (_moveDirection.magnitude > 0.1f)
        {
            // 가속
            // _velocity = Vector3.MoveTowards(_velocity, _moveDirection * currentSpeed, Time.deltaTime * _acceleration);
            // _velocity = _moveDirection * currentSpeed;
            _velocity = Vector3.Lerp(_velocity, _moveDirection * currentSpeed, Time.deltaTime * _acceleration);
        }
        else
        {
            // 감속
            // 그냥 멈추기로
            // _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, Time.deltaTime * _deceleration);
            // _velocity = Vector3.zero;
            float smoothTime = 0.01f;
            _velocity = Vector3.SmoothDamp(_velocity, Vector3.zero, ref _velocity, smoothTime);
        }
        _characterController.Move(_velocity * Time.deltaTime);

        if (_animator != null)
        {
            // TODO: Animator 설정
        }

        if (_isLastPosition)
        {
            float distanceToLastPos = Vector3.Distance(transform.position, _lastPosition);
            if (distanceToLastPos < 0.1f)
            {
                _isLastPosition = false;
                _moveInput = Vector2.zero;
            }
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
        _isLastPosition = false;

        if (showDebugInfo)
            Debug.Log($"Move Input: {_moveInput}");
    }

    void OnAttack(InputValue value)
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
        {
            Vector3 worldMousePos = hitInfo.point;
            worldMousePos.y = transform.position.y; // 플레이어 높이에 맞춤

            Vector3 direction = (worldMousePos - transform.position).normalized;
            Debug.DrawRay(transform.position, direction * 5f, Color.green, 2f);
            Vector2 dirction2D = new Vector2(direction.x, direction.z);

            if (Vector3.Distance(worldMousePos, transform.position) < 0.1f)
            {
                // TODO: hitInfo 타겟 확인, 회전, 공격
            }
            else
            {
                _moveInput = dirction2D;
                _lastPosition = worldMousePos;
                _isLastPosition = true;
            }            
        }
    }

    // 외부에서 호출 가능한 메서드들
    public Vector3 GetVelocity() => _velocity;
    public bool IsMoving() => _velocity.magnitude > 0.1f;
}
