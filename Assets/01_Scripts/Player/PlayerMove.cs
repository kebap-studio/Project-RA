using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3인칭 카메라 기반 플레이어 이동 - PlayerCharacter와 연동
/// </summary>
public class PlayerMove : MonoBehaviour
{
    private PlayerCharacter playerCharacter;
    private PlayerInput playerInput;

    // ===== Input =====
    private Vector2 moveInput;
    private bool sprintHeld = false;

    void Start()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
        playerInput = GetComponent<PlayerInput>();

        if (playerCharacter == null)
            Debug.LogError("PlayerCharacter 컴포넌트가 없습니다.");
        if (playerInput == null)
            Debug.LogWarning("PlayerInput 컴포넌트가 없습니다.");
    }

    void Update()
    {
        if (playerCharacter == null || !playerCharacter.IsAlive()) return;

        // Sprint 상태 확인
        if (playerInput != null && playerInput.actions != null)
        {
            var sprintAction = playerInput.actions["Sprint"];
            if (sprintAction != null)
                sprintHeld = sprintAction.IsPressed();
        }

        // 스프린트 상태 전달
        playerCharacter.SetSprint(sprintHeld);

        // 이동 방향 전달 (카메라 방향 변환은 PlayerCharacter.Move()에서 처리)
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
        playerCharacter.Move(moveDir);
    }

    // Action: Move (Value/Vector2)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Action: Sprint (Button)
    public void OnSprint(InputValue value)
    {
        sprintHeld = value.isPressed;
    }
}
