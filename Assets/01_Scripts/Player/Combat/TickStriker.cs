using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

// 도트딜 : 주기마다 지속 딜
public class TickStriker : MonoBehaviour, IStriker
{
    // vfx이펙트 리스트
    [SerializeField] private List<VisualEffect> visualEffectAssets;
    
    // 옵션
    [SerializeField] private int attackCount;
    [SerializeField] private int tps; // 초당 공격횟수
    
    private float _tickInterval; // 공격 반복 주기 시간

    void Awake()
    {
        _tickInterval = 1.0f / tps;
    }

    void Start()
    {
        foreach (var vfx in visualEffectAssets)
        {
            if (vfx)
            {
                vfx.Play();
            }
        }

        StartCoroutine(UpdateAttack());
    }

    IEnumerator UpdateAttack()
    {
        int count = 0;
        while (count < attackCount)
        {
            count++;
            OnAttack();
            yield return new WaitForSeconds(_tickInterval);
        }
        OnFinished();
    }

    public void OnAttack()
    {
        /*
         여기서 충돌판정은 여러가지가 나올수 있다.
         기본적인 box, capsule, sphere가 있고,
         설마 그러지는 않겠지만 몬스터 n개 거리조절로 충돌 입히는거,
         전체맵 공격등 여러가지가 가능할듯 하다.
         */
    }

    public void OnFinished()
    {
        foreach (var vfx in visualEffectAssets)
        {
            if (vfx)
            {
                vfx.Stop();
            }
        }
    }
}
