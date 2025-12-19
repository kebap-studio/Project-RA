using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody rb;
    private Animator anim;
    private PlayerInput playerInput;

    [Header("Move")]
    public float walkSpeed = 6f;
    public float runSpeed = 10f;

    [Header("Acceleration")]
    public float acceleration = 6f;   // 가속 속도
    public float deceleration = 8f;   // 감속 속도

    [Header("Turn Speed")]
    public float turnSpeed = 360f;        // 기본 회전 속도(도/초)
    public float turnSpeedBoost = 1080f;  // 급회전 시 최대 회전 속도(도/초)

    [Header("Jump")]
    public float jumpHeight = 3f;

    [Header("Ground Check")]
    public LayerMask layer;

    // ===== New Input System =====
    private Vector2 moveInput;        // (x,z) 입력
    private bool sprintHeld = false;  // Dash(Shift) 홀드
    private bool jumpPressed = false; // 점프 1회 입력(물리에서 처리)

    // ===== Movement =====
    private Vector3 dir = Vector3.zero;
    private bool isGround = false;

    // 애니/이동 가속용
    private float currentMoveSpeed01 = 0f;
    private float targetMoveSpeed01 = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
            Debug.LogWarning("PlayerInput 컴포넌트가 없습니다. (Shift 홀드 감지가 불안정할 수 있어요)");
    }

    void Update()
    {
        // Dash(Shift) 홀드 상태는 매 프레임 직접 확인
        if (playerInput != null && playerInput.actions != null)
        {
            var dashAction = playerInput.actions["Dash"];
            if (dashAction != null)
                sprintHeld = dashAction.IsPressed();
        }

        // 입력 벡터 -> 월드 이동 방향(x,z)
        dir = new Vector3(moveInput.x, 0f, moveInput.y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        CheckGround();

        // ===== 목표 MoveSpeed 계산 =====
        if (dir.sqrMagnitude < 0.001f)
        {
            targetMoveSpeed01 = 0f; // Idle
        }
        else
        {
            // 기본 Walk(0.4), Sprint 홀드면 Run(1.0)
            targetMoveSpeed01 = sprintHeld ? 1.0f : 0.4f;
        }

        // ===== 가속 / 감속 처리 =====
        float accel = targetMoveSpeed01 > currentMoveSpeed01 ? acceleration : deceleration;
        currentMoveSpeed01 = Mathf.MoveTowards(
            currentMoveSpeed01,
            targetMoveSpeed01,
            accel * Time.deltaTime
        );

        // ===== Animator =====
        if (anim != null)
        {
            anim.SetFloat("MoveSpeed", currentMoveSpeed01);
        }
    }

    void FixedUpdate()
    {
        // ===== 회전: 자연스럽지만 빠르게 =====
        if (dir != Vector3.zero)
        {
            float angle = Vector3.Angle(transform.forward, dir);

            // 각도가 클수록 더 빠르게 회전
            float t = Mathf.InverseLerp(0f, 120f, angle);
            float currentTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeedBoost, t);

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                currentTurnSpeed * Time.fixedDeltaTime
            );
        }

        // ===== 이동 =====
        float currentSpeed = Mathf.Lerp(walkSpeed, runSpeed, currentMoveSpeed01);
        rb.MovePosition(transform.position + dir * currentSpeed * Time.fixedDeltaTime);

        // ===== 점프(물리 처리) =====
        if (jumpPressed)
        {
            jumpPressed = false;

            if (isGround)
            {
                rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);

                if (anim != null)
                    anim.SetTrigger("Jump");
            }
        }
    }

    void CheckGround()
    {
        isGround = Physics.Raycast(
            transform.position + (Vector3.up * 0.2f),
            Vector3.down,
            0.4f,
            layer
        );
    }

    // Action: Move (Value/Vector2)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Action: Jump (Button)
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressed = true;
    }

    // Action: Dash (Button)
    public void OnDash(InputValue value)
    {
        // 선택: 즉시 반응을 위해 남겨두되, 최종 상태는 Update가 보장
        sprintHeld = value.isPressed;
    }

    // Action: Attack
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("플레이어 공격!");
        }
    }
}
