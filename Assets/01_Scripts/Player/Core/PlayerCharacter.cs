using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// SaDo 캐릭터 - 플레이어가 조작하는 메인 캐릭터 (완전 대체)
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacter : Character
{
    #region SaDo 스탯 설정

    [Header("Player Stats")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp;
    [SerializeField] private float baseSpeed = 3.5f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("Inventory")]
    [SerializeField] private List<Item> itemList = new List<Item>();
    
    private float _currentYaw; // 언리얼의 Controller Yaw 역할

    #endregion

    #region Components

    private CharacterController _characterController;
    private Animator _animator;
    private Camera _camera;

    #endregion

    #region Movement & State

    private Vector3 _currentVelocity;
    private Vector3 _targetPosition;
    private bool _isMovingToTarget;
    private bool _isSprinting;
    private bool _isAttacking;
    private float _lastAttackTime;

    #endregion

    #region Animation Parameter IDs (SaDo Animator와 동기화)

    private int _animIDSpeed; // 이동 속도 (0-1)
    private int _animIDIsMoving; // 이동 중 여부
    private int _animIDAttack; // 공격 트리거
    private int _animIDIsSprinting; // 스프린트 여부

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        base.Awake();
        InitializeComponents();
        CacheAnimationParameters();
        SetupSaDoDatials();
    }

    private void Update()
    {
        if (!IsAlive()) return;

        HandleMovement();
        HandleTargetMovement();
        UpdateAnimationStates();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _camera = Camera.main;

        if (_characterController == null)
        {
            Debug.LogError($"[SaDo PlayerCharacter] CharacterController component not found!");
        }

        if (_animator == null)
        {
            Debug.LogError($"[SaDo PlayerCharacter] Animator component not found!");
        }

        // Character 기본값 초기화
        currentHp = maxHp;
        moveSpeed = baseSpeed;
    }

    private void CacheAnimationParameters()
    {
        // SaDo Animator의 파라미터명과 동일하게 설정
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDIsMoving = Animator.StringToHash("IsMoving");
        _animIDAttack = Animator.StringToHash("Attack");
        _animIDIsSprinting = Animator.StringToHash("IsSprinting");

        Debug.Log("[SaDo PlayerCharacter] Animation parameters cached");
    }

    private void SetupSaDoDatials()
    {
        // SaDo 캐릭터 고유 설정
        maxHealth = 150f;
        attackPower = 15f;

        // 테그 설정 (필요시)
        gameObject.tag = "Player";

        Debug.Log(
            $"[SaDo PlayerCharacter] Initialized - Health: {maxHealth}, Speed: {baseSpeed}, Attack Power: {attackPower}");
    }

    #endregion

    #region Character Implementation (Character 추상 클래스 구현)

    /// <summary>
    /// SaDo가 지정된 방향으로 이동합니다
    /// </summary>
    public void Move(Vector3 direct)
    {
        // 공격 중이거나 클릭 이동 중일 때는 입력 무시
        if (_isAttacking || _isMovingToTarget)
        {
            return;
        }
        
        Vector3 lookFoward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Vector3 lookRight = new Vector3(_camera.transform.right.x, 0, _camera.transform.right.z).normalized;
        Vector3 moveDir = lookFoward * direct.z + lookRight * direct.x;
        
        float currentSpeed = _isSprinting ? baseSpeed * sprintMultiplier : baseSpeed;
        _currentVelocity = moveDir * currentSpeed;
        
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion viewRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, viewRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// SaDo가 지정된 위치를 공격합니다
    /// </summary>
    public void Attack(Vector3 targetPosition)
    {
        if (_isAttacking || !IsAlive()) return;

        // 공격 쿨타임 체크
        if (Time.time - _lastAttackTime < attackCooldown)
        {
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 공격 범위 내: 즉시 공격
        if (distanceToTarget <= attackRange)
        {
            PerformAttack(targetPosition);
        }
        // 공격 범위 밖: 이동 후 공격
        else
        {
            MoveToTarget(targetPosition);
        }
    }

    public override void Die()
    {
        if (isDead) return;

        base.Die();

        // 모든 상태 초기화
        StopMovement();
        _isAttacking = false;
        _isMovingToTarget = false;

        // 사망 애니메이션 (필요시 구현)
        // _animator?.SetTrigger("Die");

        Debug.Log($"[SaDo PlayerCharacter] SaDo has fallen in battle");
    }

    #endregion

    #region Attack System

    private void PerformAttack(Vector3 targetPosition)
    {
        if (_animator == null || !IsAlive()) return;

        // 대상 방향으로 회전
        Vector3 lookDirection = (targetPosition - transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // 공격 상태 설정
        _isAttacking = true;
        _lastAttackTime = Time.time;
        _currentVelocity = Vector3.zero;

        // 공격 애니메이션 트리거
        _animator.SetTrigger(_animIDAttack);

        Debug.Log($"[SaDo PlayerCharacter] Attack! Power: {attackPower}");
    }

    private void MoveToTarget(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
        _isMovingToTarget = true;
        Debug.Log($"[SaDo PlayerCharacter] Moving to target: {targetPosition}");
    }

    /// <summary>
    /// 공격이 완료되었는지 확인합니다
    /// </summary>
    public bool IsAttackComplete()
    {
        if (_animator == null) return false;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // Attack 상태가 아니거나 애니메이션이 끝났으면 true
        if (!stateInfo.IsName("Attack"))
        {
            return true;
        }

        // Attack 상태인데 애니메이션이 끝났으면 true
        return stateInfo.normalizedTime >= 1.0f;
    }

    #endregion

    #region Movement System

    private void HandleMovement()
    {
        if (_characterController == null) return;

        _characterController.Move(_currentVelocity * Time.deltaTime);
    }

    private void HandleTargetMovement()
    {
        // 마우스 클릭 <= 버린다. 
        return;
        if (!_isMovingToTarget) return;

        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

        // 목표 도달
        if (distanceToTarget <= 0.2f)
        {
            _isMovingToTarget = false;
            _currentVelocity = Vector3.zero;
            PerformAttack(_targetPosition);
        }
        else
        {
            // 목표로 이동
            Vector3 direction = (_targetPosition - transform.position).normalized;
            float currentSpeed = _isSprinting ? baseSpeed * sprintMultiplier : baseSpeed;
            _currentVelocity = direction * currentSpeed;

            // 이동 방향으로 회전
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void StopMovement()
    {
        _currentVelocity = Vector3.zero;
        _isMovingToTarget = false;
    }

    #endregion

    #region Sprint System

    public void SetSprint(bool isSprinting)
    {
        _isSprinting = isSprinting;

        if (_animator != null)
        {
            _animator.SetBool(_animIDIsSprinting, isSprinting);
        }
    }

    #endregion

    #region Animation System

    private void UpdateAnimationStates()
    {
        if (_animator == null) return;

        // 이동 상태
        bool isMoving = IsMoving();
        float normalizedSpeed = GetNormalizedSpeed();

        _animator.SetBool(_animIDIsMoving, isMoving);
        _animator.SetFloat(_animIDSpeed, normalizedSpeed);

        // 스프린트 상태는 SetSprint에서 처리
    }

    #endregion

    #region Public Methods - State Queries

    public bool IsMoving() => _currentVelocity.magnitude > 0.1f;

    public bool IsAttacking() => _isAttacking;

    public bool IsSprinting() => _isSprinting;

    public Vector3 GetVelocity() => _currentVelocity;

    public float GetNormalizedSpeed()
    {
        float currentMaxSpeed = _isSprinting ? baseSpeed * sprintMultiplier : baseSpeed;
        return currentMaxSpeed > 0 ? Mathf.Clamp01(_currentVelocity.magnitude / currentMaxSpeed) : 0f;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        Debug.Log($"플레이어 피해: {damage}, 현재 HP: {currentHp}");

        if (currentHp == 0)
        {
            Die();
        }
    }

    public int GetCurrentHp()
    {
        return currentHp;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetAttackRange()
    {
        return attackRange;
    }
    
    #endregion

    #region Animation Events (Animator에서 호출)

    /// <summary>
    /// 공격 애니메이션의 공격 판정 프레임에서 호출
    /// </summary>
    public void OnAttackHit()
    {
        ProcessAttackHit();
    }

    /// <summary>
    /// 공격 애니메이션 종료 시 호출
    /// </summary>
    public void OnAttackComplete()
    {
        _isAttacking = false;

        if (_animator != null)
        {
            // 공격 완료 후 대기 상태로 복귀
            _animator.ResetTrigger(_animIDAttack);
        }

        Debug.Log("[SaDo PlayerCharacter] Attack complete");
    }

    private void ProcessAttackHit()
    {
        // 공격 범위 내의 적 찾기
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                var enemy = collider.GetComponent<Character>();
                if (enemy != null && enemy != this)
                {
                    enemy.TakeDamage(GetAttackPower());
                    Debug.Log($"[SaDo PlayerCharacter] Hit {enemy.name} for {GetAttackPower()} damage");
                }
            }
        }
    }

    #endregion
}
