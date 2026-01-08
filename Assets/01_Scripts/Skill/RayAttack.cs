using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

public class RayAttack : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private List<Transform> points = new List<Transform>();
    [SerializeField] private float tick = 0.5f; 
    [SerializeField] private float time = 5.0f;
    [SerializeField] private int attCount = 1;
    
    private List<Vector3> _prePoints = new List<Vector3>();
    private HashSet<int> _hitSet = new HashSet<int>();
    private bool _useAttack = true;
    
    void Start()
    {
        Assert.IsTrue(points.Count > 0);
        for (int i = 0; i < points.Count; i++)
        {
            _prePoints.Add(points[i].position);
        }
    }

    void Update()
    {
        if (_useAttack == false) return;
        
        /*
         1. delay와 update사용?
         2. update만으로만 정교하게?
         3. 코루틴 delay?
         */
        tick -= Time.deltaTime;
        if (tick >= 0.0f) return;
        tick = 0.5f;
        
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
                    Debug.Log(hit.collider.gameObject.GetInstanceID());
                }
                trigger = true;
            }
            
            if (trigger)
                OnDrawRays(_prePoints[i], dir, dist, Color.red);
            else
                OnDrawRays(_prePoints[i], dir, dist, Color.green);
        }
        
        _prePoints = curPoints;
    }

    public bool UseAttack()
    {
        return _useAttack;
    }

    private void OnDrawRays(Vector3 start, Vector3 direction, float distance, Color color)
    {
        // 1.0f 가 1초 동안 유지하라는 의미입니다.
        Debug.DrawRay(start, direction.normalized * distance, color, 2.0f);
    }
}
