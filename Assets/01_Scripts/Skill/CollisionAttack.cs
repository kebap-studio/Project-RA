using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAttack : MonoBehaviour, IAttack, IPoolable
{
    // 일단 캡슐로만 가자...
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float radius;
    [SerializeField] private float height;
    [SerializeField] private float duration = 1f; // 유지시간 const 안되는데...
    [SerializeField] private int attCount = 1;

    private HashSet<int> _hitSet = new HashSet<int>();
    private bool _useAttack = true;
    private bool _isColliding = false;
    private float _time = 0.0f;
    private Vector3 _top, _bottom;

    void Start()
    {
        GetCapsulePoints(out _top, out _bottom);
        // Restart();
    }

    public bool UseAttack()
    {
        return _useAttack;
    }

    public void Restart(int durationTime = -1)
    {
        Stop();
        if (durationTime == -1)
            _time = duration;
        else
            _time = durationTime;
        _useAttack = true;
        _isColliding = false;
        StartCoroutine(CheckCollision());
    }

    public void Stop()
    {
        if (_isColliding)
            StopCoroutine(CheckCollision());
    }

    private IEnumerator CheckCollision()
    {
        GetCapsulePoints(out _top, out _bottom);
        _isColliding = true;
        while (_time >= 0)
        {
            Collider[] colliders = new Collider[attCount];
            int size = Physics.OverlapCapsuleNonAlloc(_top, _bottom, radius, colliders, layerMask);

            bool trigger = false;
            for (int j = 0; j < size; j++)
            {
                Collider col = colliders[j];
                if (_hitSet.Add(col.gameObject.GetInstanceID()))
                {
                    // 어택 성공
                    // 이벤트를 호출시켜야 되는데.. 캐스팅 없이 안되는 거겠지....
                    Debug.Log($"공격 성공 {col.gameObject.GetInstanceID()} !!!!");
                    Character character = col.gameObject.GetComponent<Character>();
                    if (character != null)
                    {
                        character.TakeDamage(0);
                    }
                }
                trigger = true;
            }

            if (trigger)
                OnDrawCapsule(radius, _top, _bottom, Color.red);
            else
                OnDrawCapsule(radius, _top, _bottom, Color.green);

            _time -= Time.deltaTime;
            yield return new WaitForSeconds(0.05f);
        }
        _isColliding = false;
        OnPoolRelease();
    }

    void GetCapsulePoints(out Vector3 p1, out Vector3 p2)
    {
        // 1. 중심에서 구체 중심까지의 거리 계산
        // 전체 높이의 절반에서 반지름을 빼야 딱 맞는 캡슐 모양이 나옵니다.
        float halfHeightMinusRadius = (height / 2f) - radius;

        // 2. 앞방향(forward)을 기준으로 좌표 계산
        // p1은 앞쪽(Forward), p2는 뒤쪽(Back)
        p1 = transform.position + transform.forward * halfHeightMinusRadius;
        p2 = transform.position - transform.forward * halfHeightMinusRadius;
    }

    // 아래 ai로 임시로 때움
    private void OnDrawCapsule(float r, Vector3 point1, Vector3 point2, Color color)
    {
        float duration = 0.3f;

        // 1. 양 끝점에 구체 모양 그리기 (3축 기준 3개의 원)
        DrawDebugSphere(point1, r, color, duration);
        DrawDebugSphere(point2, r, color, duration);

        // 2. 옆면 기둥 선 그리기
        Vector3 dir = (point2 - point1).normalized;
        // 방향에 수직인 벡터 2개 계산
        Vector3 up = Vector3.Cross(dir, Vector3.up).sqrMagnitude < 0.001f
            ? Vector3.Cross(dir, Vector3.forward).normalized
            : Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(dir, up).normalized;

        // 기둥 선 4개
        Debug.DrawLine(point1 + up * r, point2 + up * r, color, duration);
        Debug.DrawLine(point1 - up * r, point2 - up * r, color, duration);
        Debug.DrawLine(point1 + right * r, point2 + right * r, color, duration);
        Debug.DrawLine(point1 - right * r, point2 - right * r, color, duration);
    }

    // 구체 모양을 그리기 위한 헬퍼 함수
    private void DrawDebugSphere(Vector3 center, float radius, Color color, float duration)
    {
        float segments = 12; // 원을 구성할 선의 개수
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / segments) * Mathf.PI * 2;
            float nextAngle = ((i + 1) / segments) * Mathf.PI * 2;

            // XZ 평면 원
            Debug.DrawLine(center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius),
                center + new Vector3(Mathf.Cos(nextAngle) * radius, 0, Mathf.Sin(nextAngle) * radius),
                color,
                duration);
            // XY 평면 원
            Debug.DrawLine(center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0),
                center + new Vector3(Mathf.Cos(nextAngle) * radius, Mathf.Sin(nextAngle) * radius, 0),
                color,
                duration);
            // YZ 평면 원
            Debug.DrawLine(center + new Vector3(0, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius),
                center + new Vector3(0, Mathf.Cos(nextAngle) * radius, Mathf.Sin(nextAngle) * radius),
                color,
                duration);
        }
    }

    public void OnPoolRelease()
    {
        ObjectPoolManager.Instance.Push(this);
    }
}
