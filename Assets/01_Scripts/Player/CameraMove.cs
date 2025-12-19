using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform player;

    // 카메라 위치 오프셋 (최종 구도 기준)
    private Vector3 offset = new Vector3(0f, 8f, -7f);

    // 고정할 카메라 회전값 (3/4 뷰, 45도)
    private Quaternion fixedRotation;

    void Start()
    {
        // 시작 시 카메라 회전을 한 번만 고정
        fixedRotation = Quaternion.Euler(45f, 0f, 0f);
        transform.rotation = fixedRotation;
    }

    void LateUpdate()
    {
        // 위치만 플레이어를 따라감
        transform.position = player.position + offset;

        // 회전은 항상 고정 (혹시 다른 스크립트가 건드릴 경우 대비)
        transform.rotation = fixedRotation;
    }
}
