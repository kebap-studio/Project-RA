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
        _moveDirection = Vector3.zero;

        if (_isLastPosition)
        {
            float distanceToLastPos = Vector3.Distance(transform.position, _lastPosition);
            _moveDirection = Vector3.Normalize(_lastPosition - transform.position);

            if (distanceToLastPos <= 0.1f)
            {
                _isLastPosition = false;
                _moveInput = Vector2.zero;
                _velocity = Vector3.zero;
                return;
            }
        }
        else
        {
            _moveDirection = new Vector3(normalizedInput.x, 0f, normalizedInput.y);
        }   
        _velocity = _moveDirection * moveSpeed;

        if (_animator != null)
        {
            // TODO: Animator 설정
        }

        _animator.SetFloat("MoveSpeed", MoveSpeed());
        _characterController.Move(_velocity * Time.deltaTime);
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

            if (Vector3.Distance(worldMousePos, transform.position) < 0.1f)
            {
                // TODO: hitInfo 타겟 확인, 회전, 공격
                _lastPosition = transform.position;
                _isLastPosition = false;
            }
            else
            {
                _moveInput = Vector2.zero;
                _lastPosition = worldMousePos;
                _isLastPosition = true;
            }
        }
    }

    void OnSkill(InputValue value, int code)
    {
        if (_animator == null) return;

        if (!value.isPressed) return;

        if (_isMoving)
        {
            _isLastPosition = false;
            _isMoving = false;
            _animator.SetBool("IsMoving", _isMoving);
        }

        _animator.SetBool("IsAttack", true);
        _animator.SetBool("IsAttack", true);
        _animator.SetBool("IsAttack", true);
        _animator.SetBool("IsAttack", true);
        _animator.SetInteger("MotionNum", code);
    }
    
    void OnSkill1(InputValue value)
    {
        OnSkill(value, 1);
    }
    void OnSkill2(InputValue value)
    {
        OnSkill(value, 2);
    }
    void OnSkill3(InputValue value)
    {
        OnSkill(value, 3);
    }
    void OnSkill4(InputValue value)
    {
        OnSkill(value, 4);
    }

    // 외부에서 호출 가능한 메서드들
    public Vector3 GetVelocity() => _velocity;
    public bool IsMoving() => _velocity.magnitude > 0.1f;
    public float MoveSpeed() => Mathf.Clamp(_velocity.magnitude / moveSpeed, 0.0f, 1.0f);
}
