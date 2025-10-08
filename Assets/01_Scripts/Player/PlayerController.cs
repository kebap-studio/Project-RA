using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
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
    private bool _isSprinting;

    // Movement
    private Vector3 _moveDirection;
    private Vector3 _velocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            Debug.Log("PlayerInput 연결됨!");
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();

        if (showDebugInfo)
        {
            DebugInfo();
        }
    }

    private void HandleMovement()
    {
        // 입력을 3D 월드 좌표로 변환 (2.5D 탑다운)
        _moveDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        float currentSpeed = _isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

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

    // Input System Events
    void OnMove(InputValue value) // 함수명이 액션과 일치해야 함
    {
        _moveInput = value.Get<Vector2>();

        if (showDebugInfo)
            Debug.Log($"Move Input: {_moveInput}");
    }

    public void OnSprint(InputValue value)
    {
        _isSprinting = value.isPressed;

        if (showDebugInfo)
            Debug.Log($"Sprint Input: {_isSprinting}");
    }

    // 외부에서 호출 가능한 메서드들
    public Vector3 GetVelocity() => _velocity;
    public bool IsMoving() => _velocity.magnitude > 0.1f;
    public bool IsSprinting() => _isSprinting;
}
