using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RayAttack : MonoBehaviour, IAttack, IPoolable
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private List<Transform> points = new List<Transform>();
    [SerializeField] private float tick = 0.5f; // 제거
    [SerializeField] private float duration = 5.0f;
    [SerializeField] private int attCount = 1;

    private List<Vector3> _prePoints = new List<Vector3>();
    private HashSet<int> _hitSet = new HashSet<int>();
    private bool _useAttack = false;
    private bool _isColliding = false;
    private float _time = 0.0f;

    void Start()
    {
        Debug.Assert(points.Count > 0, "[RayAttack] points list is empty!");
        for (int i = 0; i < points.Count; i++)
        {
            _prePoints.Add(points[i].position);
        }
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
        _isColliding = true;
        while (_time >= 0)
        {
            List<Vector3> curPoints = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
            {
                curPoints.Add(points[i].position);

                float dist = Vector3.Distance(_prePoints[i], curPoints[i]);
                Vector3 dir = curPoints[i] - _prePoints[i];

                RaycastHit[] hits = new RaycastHit[attCount];
                int size = Physics.RaycastNonAlloc(_prePoints[i], dir, hits, dist, layerMask);

                bool trigger = false;
                for (int j = 0; j < size; j++)
                {
                    RaycastHit hit = hits[j];
                    if (_hitSet.Add(hit.collider.gameObject.GetInstanceID()))
                    {
                        // 어택 성공
                        // 이벤트를 호출시켜야 되는데.. 캐스팅 없이 안되는 거겠지....
                        Debug.LogFormat($"어택 성공 {hit.collider.gameObject.GetInstanceID()} !!!");
                    }
                    trigger = true;
                }

                if (trigger)
                    OnDrawRays(_prePoints[i], dir, dist, Color.red);
                else
                    OnDrawRays(_prePoints[i], dir, dist, Color.green);
            }

            _time -= Time.deltaTime;
            _prePoints = curPoints;
            yield return null;
        }
        _isColliding = false;
        OnPoolRelease();
    }

    private void OnDrawRays(Vector3 start, Vector3 direction, float distance, Color color)
    {
        // 1.0f 가 1초 동안 유지하라는 의미입니다.
        Debug.DrawRay(start, direction.normalized * distance, color, 2.0f);
    }

    public void OnPoolRelease()
    {
        ObjectPoolManager.Instance.Push(this);
    }
}
