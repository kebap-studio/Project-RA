using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DieState : MonoBehaviour, IState
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
        return EStateType.DIE;
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
        // 사망 애니메이션 실행을 위해 잠깐 정지 시킨다.
        yield return new WaitForSeconds(1.0f);
        onFinished?.Invoke();
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
