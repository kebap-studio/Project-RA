using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


/// <summary>
/// 기본 몬스터 - 1 캐릭터 - AI로 조작하는 메인 몬스터
/// </summary>
public class MonsterCharacter : Character
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
    [SerializeField] private AttackState attackState;
    [SerializeField] private HittedState hittedState;
    [SerializeField] private DieState dieState;
    private IState _currentState;

    [Header("Prefabs")]
    [SerializeField] private DamageFontUI damageUIPrefab;

    #endregion

    #region Components

    private NavMeshAgent _navMeshAgent;

    #endregion

    #region Movement & State

    private Vector3 _currentVelocity;
    private Vector3 _targetPosition;
    private bool _isMovingToTarget;
    private bool _isSprinting;
    private bool _isAttacking;
    private float _lastAttackTime;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        base.Awake();
        InitializeComponents();
        SetupDetials();

        // TODO : 만약 몹이 수백개 되면 최적화 고려
        stateContext = new MonsterStateContext(this);
        idleState.init(stateContext, () => UpdateState(moveState));
        moveState.init(stateContext);
        chaseState.init(stateContext,
            () =>
            {
                Debug.LogFormat("추적상태 완료함수 람다식 사용할게요 !!!");
                UpdateState(attackState);
            });
        attackState.init(stateContext,
            () =>
            {
                Debug.LogFormat("공격완료상태 완료함수 람다식 사용할게요 !!!");
                UpdateState(idleState);
            });
        _currentState = idleState;
    }

    private void Start()
    {
        stateContext.Init(_currentState);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agent.updateRotation = false;
        }
    }

    private void Update()
    {
        // 맞거나, 죽는 상태이면 스킵한다.
        if (_currentState.GetEStateType() == EStateType.DIE || _currentState.GetEStateType() == EStateType.HITTED)
        {
            return;
        }

        // 플레이어 체크 : GameManager에서 변수로 등록해두면 바로 들고올수 있어 좋을거 같긴한다. readonly로
        var player = GameObject.FindWithTag("Player");
        Vector3 playerPosition = new Vector3(1000, 0, 1000);
        if (player)
            playerPosition = player.transform.position;

        if (_currentState.GetEStateType() == EStateType.MOVE)
        {
            // 플레이어의 거리가 가까우면 추적상태
            RaycastHit hit = new RaycastHit();
            if (Vector3.Distance(playerPosition, transform.position) < 20.0f)
            {
                UpdateState(chaseState);
                // Physics.Raycast(transform.position, playerPosition - transform.position, out hit, 20.0f);
                // if (hit.collider.gameObject.CompareTag("Player"))
                // {
                //     UpdateState(chaseState);
                // }
                // else
                // {
                //     UpdateState(idleState);
                // }
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
            if (Vector3.Distance(playerPosition, transform.position) > 20.0f)
            {
                UpdateState(idleState);
            }
        }
    }

    private void LateUpdate()
    {
        if (_navMeshAgent.hasPath && !_navMeshAgent.isStopped)
        {
            Vector3 dir = _navMeshAgent.steeringTarget - transform.position;
            dir.y = 0;

            // 예상 도착지점까지 오면 회전은 안해도 이상하지는 않음(사실상 그런 상황 없을듯)
            if (dir.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir.normalized),
                    Time.deltaTime * _navMeshAgent.angularSpeed);
                transform.localRotation = targetRotation;
            }
        }

    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (_navMeshAgent == null)
        {
            Debug.LogError($"[Monster] NavMesh agent component not found!");
        }

        // Character 기본값 초기화
        moveSpeed = baseSpeed;
    }

    private void SetupDetials()
    {
        // Monster 캐릭터 고유 설정
        maxHealth = 150f;
        attackPower = 15f;

        // 테그 설정 (필요시)
        // gameObject.tag = "Monster";

        Debug.Log(
            $"[Monster PlayerCharacter] Initialized - Health: {maxHealth}, Speed: {baseSpeed}, Attack Power: {attackPower}");
    }

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

    #region Override Methods

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        // 데미지 폰트 띄운다.
        DamageFontUI damageFontUI = ObjectPoolManager.Instance.Pop(damageUIPrefab);
        damageFontUI.SetDamage((int) damage, transform.position);

        if (this.currentHealth <= 0)
        {
            UpdateState(dieState);
        }
        else
        {
            UpdateState(hittedState);
        }
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
}
