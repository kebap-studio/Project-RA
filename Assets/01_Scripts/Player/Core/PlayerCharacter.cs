using UnityEngine;


/// <summary>
/// Player가 조작하는 캐릭터의 구현체
/// </summary>
[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerCharacter : Character
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2f;

    // Components
    CharacterController _characterController;
    Animator _animator;

    // Movement
    private Vector3 _currentVelocity;
    private Vector3 _targetPosition;
    private bool _isMovingToTarget;
    private bool _isSprinting;

    // Animation IDs 
    int _animIDMoveSpeed;
    int _animIDIsMoving;
    int _animIDIsAttack;
    int _animIDMotionNum;
    int _animIDIsSprinting;

    private bool _isAttacking;

    #region Unity Lifecycle

    private void Awake()
    {
        base.Awake();
        InitializeComponents();
        CacheAnimationIDs();
    }

    private void Update()
    {
        HandleMovement();
        HandleTargetMovement();
        UpdateAnimations();
    }

    #endregion
    
    #region Initialization

    private void InitializeComponents()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        // Character 기본 값 설정
        moveSpeed = baseSpeed;

        if (_characterController == null)
        {
            Debug.LogError($"[{nameof(PlayerCharacter)}] CharacterController component not found on {gameObject.name}");
        }

        if (_animator == null)
        {
            Debug.LogError($"[{nameof(PlayerCharacter)}] Animator component not found on {gameObject.name}");
        }
    }

    private void CacheAnimationIDs()
    {
        _animIDMoveSpeed = Animator.StringToHash("MoveSpeed");
        _animIDIsMoving = Animator.StringToHash("IsMoving");
        _animIDIsAttack = Animator.StringToHash("IsAttack");
        _animIDMotionNum = Animator.StringToHash("MotionNum");
        _animIDIsSprinting = Animator.StringToHash("IsSprinting");
    }

    #endregion

    #region Character Implementation

    public override void Move(Vector3 direction)
    {
        Debug.Log($"<color=cyan>[PlayerCharacter.Move] Called with direction: {direction}</color>");
        if (_isAttacking || _isMovingToTarget)
        {
            
            Debug.LogWarning($"<color=red>[PlayerCharacter.Move] BLOCKED - isAttacking: {_isAttacking}, isMovingToTarget: {_isMovingToTarget}</color>");
            return;
        }

        float currentSpeed = _isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        _currentVelocity = direction * currentSpeed;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public override void Attack(Vector3 targetPosition)
    {
        if (_isAttacking || !IsAlive()) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget <= attackRange)
        {
            // 타겟 방향으로 회전
            Vector3 lookDirection = (targetPosition - transform.position).normalized;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            // 즉시 공격
            PerformAttack();
        }
        else
        {
            // 타겟 위치로 이동 후 공격
            MoveToTarget(targetPosition);
        }
    }

    public override void TakeDamage(float damage)
    {
        if (!IsAlive()) return;

        base.TakeDamage(damage);

        if (_animator != null)
        {
            // _animator.SetTrigger("TakeDamage");
        }
    }

    public override void Die()
    {
        base.Die();

        StopMovement();
        _isAttacking = false;

        if (_animator != null)
        {
            // _animator.SetTrigger("Die");
        }
    }

    #endregion
    
    #region Skills

    /// <summary>
    /// 스킬을 사용합니다
    /// </summary>
    public void UseSkill(int skillNumber)
    {
        if (_animator == null || _isAttacking || !IsAlive()) return;

        if (IsMoving()) StopMovement();

        _isAttacking = true;

        // 스킬 애니메이션 트리거
        // _animator.SetBool(_animIDIsAttack, true);
        // _animator.SetInteger(_animIDMotionNum, skillNumber);
    }

    public void SetSprint(bool isSprinting)
    {
        _isSprinting = isSprinting;
        
        if (_animator != null)
        {
            _animator.SetBool(_animIDIsSprinting, _isSprinting);
        }
    }

    #endregion

    #region Movement

    private void HandleMovement()
    {
        if (_characterController == null) 
        {
            Debug.LogError("[PlayerCharacter] CharacterController is NULL!");
            return;
        }
        
        _characterController.Move(_currentVelocity * Time.deltaTime);
    }

    private void HandleTargetMovement()
    {
        if (!_isMovingToTarget) return;

        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

        if (distanceToTarget <= 0.1f)
        {
            _isMovingToTarget = false;
            _currentVelocity = Vector3.zero;
            PerformAttack();
        }
        else
        {
            Vector3 direction = (_targetPosition - transform.position).normalized;
            float currentSpeed = _isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
            _currentVelocity = direction * currentSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void MoveToTarget(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
        _isMovingToTarget = true;
    }

    private void PerformAttack()
    {
        if (_animator == null || !IsAlive()) return;

        // _isAttacking = true;
        // _animator.SetBool(_animIDIsAttack, true);
        // _animator.SetInteger(_animIDMotionNum, 0); // 기본 공격
    }

    #endregion

    #region Animation

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        bool isMoving = IsMoving();
        float normalizedSpeed = GetNormalizedSpeed();

        // _animator.SetBool(_animIDIsMoving, isMoving);
        // _animator.SetFloat(_animIDMoveSpeed, normalizedSpeed);
    }

    #endregion

    #region Public Methods

    public void StopMovement()
    {
    }

    /// <summary>
    /// 현재 이동 중인지 확인합니다
    /// </summary>
    public bool IsMoving() => _currentVelocity.magnitude > 0.1f;

    /// <summary>
    /// 정규화된 이동 속도를 반환합니다 (0-1)
    /// </summary>
    public float GetNormalizedSpeed()
    {
        float currentMaxSpeed = _isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        return currentMaxSpeed > 0 ? Mathf.Clamp01(_currentVelocity.magnitude / currentMaxSpeed) : 0f;
    }

    /// <summary>
    /// 현재 속도를 반환합니다
    /// </summary>
    public Vector3 GetVelocity() => _currentVelocity;

    /// <summary>
    /// 공격 중인지 확인합니다
    /// </summary>
    public bool IsAttacking() => _isAttacking;

    /// <summary>
    /// 스프린트 중인지 확인합니다
    /// </summary>
    public bool IsSprinting() => _isSprinting;

    #endregion

    #region Animation Events

    /// <summary>
    /// 애니메이션에서 호출되는 공격 완료 이벤트
    /// </summary>
    public void OnAttackComplete()
    {
        if (_animator == null) return;

        _isAttacking = false;
        _animator.SetBool(_animIDIsAttack, false);
    }

    /// <summary>
    /// 애니메이션에서 호출되는 공격 히트 이벤트
    /// </summary>
    public void OnAttackHit()
    {
        // 실제 데미지 처리 로직
        ProcessAttackHit();
    }

    private void ProcessAttackHit()
    {
        // 공격 범위 내의 적들을 찾아서 데미지 처리
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                var enemy = collider.GetComponent<Character>();
                if (enemy != null && enemy != this)
                {
                    enemy.TakeDamage(GetAttackPower());
                    Debug.Log($"[{nameof(PlayerCharacter)}] Hit {enemy.name} for {GetAttackPower()} damage");
                }
            }
        }
    }

    #endregion
}
