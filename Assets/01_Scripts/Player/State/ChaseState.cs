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
        while (true)
        {
            // 플레이어 체크 : GameManager에서 변수로 등록해두면 바로 들고올수 있어 좋을거 같긴한다. readonly로
            var player = GameObject.FindWithTag("Player");
            var playerPosition = player.transform.position;
            Vector3 curPoint = _stateContenxt._current.transform.position;
            
            if (Vector3.Distance(playerPosition, curPoint) < 2.0f)
            {
                onFinished?.Invoke();
                _navMeshAgent.SetDestination(curPoint);
                yield break;
            }
            else
            {
                _navMeshAgent.SetDestination(playerPosition);
            }

            yield return null;
        }
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
