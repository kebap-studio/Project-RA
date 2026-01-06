using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MoveState : MonoBehaviour, IState
{
    private NavMeshAgent _navMeshAgent;
    private AStateContext _stateContenxt;
    private float _searchRadius = 50f;
    
    public void init(AStateContext stateContext, Action func = null)
    {
        _stateContenxt = stateContext;
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public EStateType GetEStateType()
    {
        return EStateType.MOVE;
    }

    public void EnterState()
    {
        if (_stateContenxt == null)
        {

            return;
        }
        StartCoroutine(UpdateState());
    }

    public IEnumerator UpdateState()
    {
        Vector3 nextPoint = _stateContenxt._current.transform.position;
        while (_stateContenxt.GetState().GetEStateType() == GetEStateType())
        {
            Vector3 curPoint = _stateContenxt._current.transform.position;

            // TODO : 제일큰 문제는 0.1이라는 임계값 넘어버리는 경우 생각해야됨
            if (Vector3.Distance(curPoint, nextPoint) < 0.1f)
            {
                nextPoint = SetRandomDestination(curPoint);
                _navMeshAgent.SetDestination(nextPoint);
            }

            yield return null;
        }
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
    
    Vector3 SetRandomDestination(Vector3 center)
    {
        // 1. 현재 위치를 중심으로 랜덤한 방향과 반경을 곱하여 월드상의 랜덤 좌표를 얻음
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * _searchRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        
        // 검색 결과가 유효한지 확인
        if (NavMesh.SamplePosition(randomDirection, out hit, _searchRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        // 검색 실패
        Debug.LogWarning("NavMesh 위에서 이동 가능한 랜덤한 위치를 찾지 못했습니다.");
        return center;
    }
}
