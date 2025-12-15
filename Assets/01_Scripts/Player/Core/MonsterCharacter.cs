using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 기본 몬스터 - 1 캐릭터 - AI로 조작하는 메인 몬스터
/// </summary>
[RequireComponent(typeof(Animator))]
public class MosterCharacter : Character
{
    #region 기본 몬스터 - 1 스탯 설정
    
    [Header("=== Monster-1 Character Stats ===")]
    [SerializeField] private float baseSpeed = 3.5f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float rotationSpeed = 12f;
    
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 0.8f;
    
    [Header("Monster State")]
    private MonsterStateContext stateContext;
    [SerializeField] private IdleState idleState;
    [SerializeField] private MoveState moveState;
    [SerializeField] private ChaseState chaseState;
    private IState _currentState;
    
    #endregion

    #region Components
    
    private Animator _animator;
    
    #endregion

    #region Movement & State
    
    private Vector3 _currentVelocity;
    private Vector3 _targetPosition;
    private bool _isMovingToTarget;
    private bool _isSprinting;
    private bool _isAttacking;
    private float _lastAttackTime;
    
    #endregion

    #region Animation Parameter IDs (Monster Animator와 동기화)
    
    private int _animIDSpeed;           // 이동 속도 (0-1)
    private int _animIDIsMoving;        // 이동 중 여부
    private int _animIDAttack;          // 공격 트리거
    private int _animIDIsSprinting;     // 스프린트 여부
    
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        base.Awake();
        InitializeComponents();
        CacheAnimationParameters();
        SetupDetials();

        // TODO : 만약 몹이 수백개 되면 최적화 고려
        stateContext = new MonsterStateContext(this);
        // idleState = new IdleState();
        // moveState = new MoveState();
        // chaseState = new ChaseState();
        idleState.init(stateContext, () => UpdateState(moveState));
        moveState.init(stateContext);
        chaseState.init(stateContext, () => {Debug.LogFormat("ATTCK !!!"); UpdateState(idleState);});
        _currentState = idleState;
    }

    private void Start()
    {
        stateContext.Init(_currentState);
    }

    private void Update()
    {
        // 플레이어 체크 : GameManager에서 변수로 등록해두면 바로 들고올수 있어 좋을거 같긴한다. readonly로
        var player = GameObject.FindWithTag("Player");
        Vector3 playerPosition = new Vector3(1000, 0, 1000);
        if (player)
            playerPosition = player.transform.position;

        if (_currentState.GetEStateType() == EStateType.MOVE)
        {
            // 플레이어의 거리가 가까우면 추적상태
            if (Vector3.Distance(playerPosition, transform.position) < 100.0f)
            {
                UpdateState(chaseState);
            }
            else
            {
                // 이동 or idle
                if (Random.Range(0, 5) == 0)
                {
                    UpdateState(idleState);
                }
            }
        }
        else if (_currentState.GetEStateType() == EStateType.CHASE)
        {
            // 플레이어의 거리가 멀어지면 잠깐 멈춘다.
            if (Vector3.Distance(playerPosition, transform.position) > 100.0f)
            {
                UpdateState(idleState);
            }
        }

        // stateContext.GetState().UpdateState();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
        {
            Debug.LogError($"[Monster PlayerCharacter] Animator component not found!");
        }

        // Character 기본값 초기화
        moveSpeed = baseSpeed;
    }

    private void CacheAnimationParameters()
    {
        // Monster Animator의 파라미터명과 동일하게 설정
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDIsMoving = Animator.StringToHash("IsMoving");
        _animIDAttack = Animator.StringToHash("Attack");
        _animIDIsSprinting = Animator.StringToHash("IsSprinting");

        Debug.Log($"[Monster PlayerCharacter] Animation parameters cached");
    }

    private void SetupDetials()
    {
        // Monster 캐릭터 고유 설정
        maxHealth = 150f;
        attackPower = 15f;
        
        // 테그 설정 (필요시)
        gameObject.tag = "Monster";
        
        Debug.Log($"[Monster PlayerCharacter] Initialized - Health: {maxHealth}, Speed: {baseSpeed}, Attack Power: {attackPower}");
    }

    #endregion

    #region Character Implementation (Character 추상 클래스 구현)

    public override void Move(Vector3 direction){}
    public override void Attack(Vector3 targetPosition){}
    public override void Die(){}

    #endregion

    #region State System

    private void UpdateState(IState state)
    {
        if (state == null)
        {
            
            return;
        }

        stateContext.ChangeState(state);
        _currentState = state;
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

        Debug.Log("[Monster PlayerCharacter] Attack complete");
    }

    private void ProcessAttackHit()
    {
        // 공격 범위 내의 적 찾기
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                var enemy = collider.GetComponent<Character>();
                if (enemy != null && enemy != this)
                {
                    enemy.TakeDamage(GetAttackPower());
                    Debug.Log($"[Monster PlayerCharacter] Hit {enemy.name} for {GetAttackPower()} damage");
                }
            }
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // 공격 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #endregion
}
