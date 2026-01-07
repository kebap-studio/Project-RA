using UnityEngine;

public class SimpleSpringArm : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 1f, -1.5f); // 캐릭터 뒤쪽 고정 위치
    [SerializeField] private Vector3 targetRotate = new Vector3(10f, 0, 0); // 캐릭터 뒤쪽 고정 위치
    [SerializeField] private float smoothTime = .12f; // 액션감(지연 시간)

    private Vector3 currentVelocity;

    void Start()
    {
        transform.rotation = Quaternion.Euler(targetRotate);
    }

    void LateUpdate()
    {
        // 핵심: 부모(캐릭터)가 회전하면 자식인 이 오브젝트도 자동으로 돌아갑니다.
        // 우리는 그저 '원래 있어야 할 상대 위치(targetOffset)'로 
        // localPosition을 부드럽게 밀어넣어주기만 하면 됩니다.

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition, 
            targetOffset, 
            ref currentVelocity, 
            smoothTime
        );

        // 만약 카메라 각도가 부모 회전에 따라 미세하게 흔들리는걸 막고 싶다면 
        // localRotation을 Quaternion.identity로 고정할 수도 있습니다.
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, 0.1f);
    }
}
