using System.Collections;
using UnityEngine;
using System;
using UnityEngine.AI;

public class ChaseState : MonoBehaviour, IState
{
    private NavMeshAgent _navMeshAgent;
    private AStateContext _stateContenxt;
    public Action onFinished;

    public void init(AStateContext stateContext, Action func = null)
    {
        _stateContenxt = stateContext;
        if (func != null)
            onFinished += func;
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public EStateType GetEStateType()
    {
        return EStateType.CHASE;
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
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 nextPoint;
        while (true)
        {
            Vector3 curPoint = _stateContenxt._current.transform.position;
            Vector3 playerPosition = player.transform.position;
            
            // 근처까지 도착
            if (Vector3.Distance(playerPosition, curPoint) < 2.0f)
            {
                _stateContenxt.GetAnimator().SetFloat("MoveSpeed", 0.0f);
                _stateContenxt.GetAnimator().SetBool("IsMoving", false);
                onFinished?.Invoke();
                _navMeshAgent.SetDestination(curPoint);
                yield break;
            }
            
            // 플레이어 추적 (값이 완전히 같으면 근처까지 도달이 안됨...)
            nextPoint = playerPosition - (playerPosition - curPoint).normalized * (2.0f - 0.1f);
            _navMeshAgent.SetDestination(nextPoint);
            _stateContenxt.GetAnimator().SetFloat("MoveSpeed", 0.2f);
            _stateContenxt.GetAnimator().SetBool("IsMoving", true);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
