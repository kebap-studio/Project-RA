using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class IdleState : MonoBehaviour, IState
{
    private AStateContext _stateContenxt;
    public Action onFinished;
    
    public void init(AStateContext stateContext, Action func = null)
    {
        _stateContenxt = stateContext;
        if (func != null)
            onFinished += func;
    }

    public EStateType GetEStateType()
    {
        return EStateType.IDLE;
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
        // idle이면 애니메이션 실행을 위해 잠깐 정지 시킨다.
        yield return new WaitForSeconds(0.5f);
        onFinished?.Invoke();
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
