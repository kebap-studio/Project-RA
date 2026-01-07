using NUnit.Framework;
using UnityEngine;

public class SpringArm : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float armLength = 3.0f;
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-40f, 60f);

    [Header("충돌 설정")]
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask collisionMask;
    
    [Header("카메라 설정")]
    [SerializeField] private Camera camera;
    [SerializeField] private Vector3 rotate = new Vector3(10f, 0f, 0f);
    
    private float _curPitch = 0f; 
    private float _curYaw = 0f;

    void Awake()
    {
        if (camera != null)
        {
            camera.transform.localPosition = new Vector3(0f, 1f, -1f * armLength);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        
        // 현재 암(또는 캐릭터)이 바라보는 월드 회전값을 초기 누적값으로 설정
        Vector3 currentRotation = transform.eulerAngles;
        _curYaw = currentRotation.y;
        _curPitch = currentRotation.x;
    
        // 유니티 오일러 보정 (360도 방지)
        if (_curPitch > 180f) 
            _curPitch -= 360f;
    }

    void Update()
    {
        // 일단 임시로 구버전 인풋 사용
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 뚝뚝 끊기는 현상 발생
        // Vector3 euler = transform.rotation.eulerAngles;
        // float x = euler.x - mouseY;
        // if (x > 360f) x -= 360f;
        // else if (x < 0f) x += 360f;
        // x = Mathf.Clamp(x, pitchLimits.x, pitchLimits.y);
        //
        // float y = euler.y + mouseX;
        // if (y > 360f) y -= 360f;
        // else if (y < 0f) y += 360f;
        
        // 1. 내부 변수에 마우스 입력값을 계속 누적 (오차 없음)
        _curYaw += mouseX;
        _curPitch -= mouseY; // 마우스를 올리면 각도가 내려가야 위를 봄

        // 2. 누적된 변수에 Clamp 적용 (범위가 명확함)
        _curPitch = Mathf.Clamp(_curPitch, pitchLimits.x, pitchLimits.y);
        
        transform.rotation = Quaternion.Euler(_curPitch, _curYaw, 0);
    }
}
