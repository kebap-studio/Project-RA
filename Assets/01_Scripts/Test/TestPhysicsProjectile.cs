using System;
using UnityEngine;

public class TestPhysicsProjectile : MonoBehaviour
{
    [SerializeField] private float MoveSpeed = 6f; // 초당 6
    [SerializeField] private float maxDist = 30f; // 초당 6

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, _startPos);

        if (dist > maxDist)
        {
            transform.position = _startPos;
            return;
        }
        
        transform.position += transform.forward * MoveSpeed * Time.deltaTime;
    }
}
