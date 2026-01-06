using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// 플레이어 입력을 처리하고 PlayerCharacter에 명령을 전달하는 컨트롤러
/// </summary>
[RequireComponent(typeof(PlayerCharacter))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private bool enableDebugInput = true;

    [Header("Mouse Settings")]
    [SerializeField] private LayerMask groundLayerMask = -1;
    [SerializeField] private float maxInteractionDistance = 100f;
    [SerializeField] private float attackRange = 2f; // 공격 범위 추가

    // Components
    private PlayerCharacter _playerCharacter;
    private Camera _mainCamera;
    private PlayerInput _playerInput;

    // Input Values
    private Vector2 _moveInput;
    private bool _isSprintHeld;
    private bool _isAttacking; // 🔧 공격 상태 추가

    // Events
    public static event Action<Vector2> OnMoveInputChanged;
    public static event Action<Vector3> OnAttackRequested;
    public static event Action<int> OnSkillRequested;
    public static event Action<bool> OnSprintChanged;


    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        SetupInputCallbacks();
    }

    private void OnEnable()
    {
        _playerInput?.ActivateInput();
    }

    private void OnDisable()
    {
        _playerInput?.DeactivateInput();
    }

    private void Update()
    {
        ProcessMovementInput();
        HandleAttackCompletion(); // 🔧 공격 종료 처리 추가

        if (enableDebugInput)
        {
            DrawDebugInfo();
        }
    }
    
    private void OnDestroy()
    {
        // 모든 구독자 해제
        OnMoveInputChanged = null;
        OnAttackRequested = null;
        OnSkillRequested = null;
        OnSprintChanged = null;
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        _playerCharacter = GetComponent<PlayerCharacter>();
        _mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        _playerInput = GetComponent<PlayerInput>();

        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (_playerCharacter == null)
        {
            Debug.LogError($"[{nameof(PlayerController)}] PlayerCharacter component not found on {gameObject.name}");
        }

        if (_mainCamera == null)
        {
            Debug.LogError($"[{nameof(PlayerController)}] Main Camera not found");
        }

        if (_playerInput == null)
        {
            Debug.LogWarning($"[{nameof(PlayerController)}] PlayerInput component not found on {gameObject.name}");
        }
    }

    private void SetupInputCallbacks()
    {
        if (_playerInput == null) return;

        Debug.Log($"[{nameof(PlayerController)}] Input System connected successfully");
    }

    #endregion

    #region Input Processing

    private void ProcessMovementInput()
    {
        if (_playerCharacter == null || !_playerCharacter.IsAlive()) return;
        
        Vector3 moveDirection = CalculateMovementDirection();
        _playerCharacter.Move(moveDirection);
    }

    private Vector3 CalculateMovementDirection()
    {
        if (_moveInput.magnitude < 0.1f) return Vector3.zero;

        Vector2 normalizedInput = _moveInput.normalized;
        return new Vector3(normalizedInput.x, 0f, normalizedInput.y);
    }

    private Vector3? GetWorldPositionFromMouse()
    {
        if (_mainCamera == null) return null;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPos);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxInteractionDistance, groundLayerMask))
        {
            Vector3 worldPos = hitInfo.point;
            worldPos.y = transform.position.y; // 플레이어 높이에 맞춤
            return worldPos;
        }

        return null;
    }

    #endregion

    #region Input System Events

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        OnMoveInputChanged?.Invoke(_moveInput);

        if (enableDebugInput)
        {
            Debug.Log($"[{nameof(PlayerController)}] Move Input: {_moveInput}");
        }
    }

    /// <summary>
    /// 🔧 수정 1: Sprint 버튼 때도 isSprint가 false가 되도록 수정
    /// </summary>
    public void OnSprint(InputValue value)
    {
        _isSprintHeld = value.Get<float>() > 0.5; // Press/Release 자동 처리
        
        if (_playerCharacter != null)
        {
            _playerCharacter.SetSprint(_isSprintHeld);
        }
        
        OnSprintChanged?.Invoke(_isSprintHeld);

        if (enableDebugInput)
        {
            Debug.Log($"[{nameof(PlayerController)}] Sprint: {(_isSprintHeld ? "Started" : "Stopped")}");
        }
    }

    /// <summary>
    /// 🔧 수정 2: Attack을 Bool 기반으로 변경
    /// </summary>
    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || _playerCharacter == null || !_playerCharacter.IsAlive()) return;

        // 이미 공격 중이면 추가 공격 방지
        if (_isAttacking)
        {
            if (enableDebugInput)
            {
                Debug.Log($"[{nameof(PlayerController)}] Attack already in progress");
            }
            return;
        }

        Vector3? targetPosition = GetWorldPositionFromMouse();
        if (targetPosition.HasValue)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition.Value);

            // 공격 시작
            _isAttacking = true;
            _playerCharacter.Attack(targetPosition.Value);
            OnAttackRequested?.Invoke(targetPosition.Value);

            if (enableDebugInput)
            {
                Debug.Log($"[{nameof(PlayerController)}] Attack requested at: {targetPosition.Value}");
            }
        }
    }

    /// <summary>
    /// 🔧 추가: 공격 종료 처리
    /// </summary>
    private void HandleAttackCompletion()
    {
        if (!_isAttacking || _playerCharacter == null) return;

        // 공격 애니메이션이 끝났는지 확인
        if (_playerCharacter.IsAttackComplete())
        {
            _isAttacking = false;

            if (enableDebugInput)
            {
                Debug.Log($"[{nameof(PlayerController)}] Attack completed");
            }
        }
    }

    public void OnSkill1(InputValue value) => HandleSkillInput(value, 1);
    public void OnSkill2(InputValue value) => HandleSkillInput(value, 2);
    public void OnSkill3(InputValue value) => HandleSkillInput(value, 3);
    public void OnSkill4(InputValue value) => HandleSkillInput(value, 4);

    private void HandleSkillInput(InputValue value, int skillNumber)
    {
        if (!value.isPressed || _playerCharacter == null || !_playerCharacter.IsAlive()) return;

        _isAttacking = false; // 🔧 공격 상태도 해제
        // _playerCharacter.UseSkill(skillNumber);
        OnSkillRequested?.Invoke(skillNumber);

        if (enableDebugInput)
        {
            Debug.Log($"[{nameof(PlayerController)}] Skill {skillNumber} used");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 입력을 활성화합니다
    /// </summary>
    public void EnableInput()
    {
        _playerInput?.ActivateInput();
    }

    /// <summary>
    /// 입력을 비활성화합니다
    /// </summary>
    public void DisableInput()
    {
        _playerInput?.DeactivateInput();
    }

    /// <summary>
    /// 현재 이동 입력값을 반환합니다
    /// </summary>
    public Vector2 GetMoveInput() => _moveInput;

    /// <summary>
    /// 스프린트 상태를 반환합니다
    /// </summary>
    public bool IsSprintHeld() => _isSprintHeld;

    /// <summary>
    /// 공격 중인지 확인합니다
    /// </summary>
    public bool IsAttacking() => _isAttacking;

    /// <summary>
    /// 플레이어 캐릭터 참조를 반환합니다
    /// </summary>
    public PlayerCharacter GetPlayerCharacter() => _playerCharacter;

    #endregion

    #region Debug

    private void DrawDebugInfo()
    {
        if (_playerCharacter == null) return;

        Vector3 playerPos = transform.position;
        Vector3 moveDirection = CalculateMovementDirection();

        // 이동 방향 표시 (빨간색)
        if (moveDirection != Vector3.zero)
        {
            Debug.DrawRay(playerPos, moveDirection * 2f, Color.red);
        }

        // 플레이어 전방 방향 표시 (파란색)
        Debug.DrawRay(playerPos, transform.forward * 2f, Color.blue);

        // 마우스 위치 표시 (초록색)
        Vector3? mouseWorldPos = GetWorldPositionFromMouse();
        if (mouseWorldPos.HasValue)
        {
            Debug.DrawLine(playerPos, mouseWorldPos.Value, Color.green);
        }

        // 공격 범위 표시 (주황색)
        DrawWireCircle(playerPos, attackRange, Color.cyan);

        // 디버그 정보 표시
        Debug.Log($"[Sprint: {_isSprintHeld}] [Attacking: {_isAttacking}]");
    }

    /// <summary>
    /// 원을 그리는 커스텀 함수 (Debug.DrawLine 사용)
    /// </summary>
    private void DrawWireCircle(Vector3 center, float radius, Color color, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPoint, newPoint, color);
            prevPoint = newPoint;
        }
    }

    #endregion
}
