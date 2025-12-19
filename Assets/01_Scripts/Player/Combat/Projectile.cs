using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

// 투사체 단일 공격 
public class Projectile : MonoBehaviour, IStriker
{
    // 타격 vfx이펙트 리스트
    [SerializeField] private List<VisualEffect> visualEffectAssets;
    
    // 옵션
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private int maxTargets = 1;
    
    // 이전 위치
    private Vector3 _previousPosition;
    
    // GameObject의 id를 저장
    private HashSet<int> _hitObjects;
    private RaycastHit[] _tempHits;

    void Awake()
    {
        _hitObjects = new HashSet<int>();
        // 일단 좀 크게 잡아둔다.(동일한게 체크될수도 있기 때문)
        _tempHits = new RaycastHit[maxTargets * 2];
    }
    
    void Start()
    {
        _previousPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        OnAttack();
    }

    public void OnAttack()
    {
        // 이번 프레임에 이동해야 할 거리와 방향 계산
        float moveDistance = speed * Time.deltaTime;
        Vector3 direction = transform.forward;

        // 이전 위치에서 다음 위치까지 미리 Ray를 쏴서 확인 (Continuous Detection)
        Ray ray = new Ray(_previousPosition, direction);
        int count = Physics.SphereCastNonAlloc(ray, radius, _tempHits, moveDistance, targetLayer);
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                int id = _tempHits[i].collider.gameObject.GetInstanceID();
                if (_hitObjects.Add(id))
                {
                    Debug.LogFormat($"{id}에게 데미지를 입혔습니다.");
                }
                
                if (_hitObjects.Count >= maxTargets)
                {
                    OnFinished();
                    Destroy(gameObject);
                    return;
                }
            }
        }
        
        transform.Translate(Vector3.forward * moveDistance);
        _previousPosition = transform.position; // 다음 프레임을 위해 현재 위치 저장
    }
    
    public void OnFinished()
    {
        foreach (var vfx in visualEffectAssets)
        {
            if (vfx)
            {
                vfx.Play();
            }
        }
    }
}
