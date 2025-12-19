using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HittedState : MonoBehaviour, IState
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
        return EStateType.HITTED;
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
        // Hitted이면 애니메이션 실행을 위해 잠깐 정지 시킨다.
        yield return new WaitForSeconds(1.0f);
        onFinished?.Invoke();
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
