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
        _stateContenxt.GetAnimator().SetBool(NPCAnimHashID.Instance.IsMoving, false);
        _stateContenxt.GetAnimator().SetBool(NPCAnimHashID.Instance.IsAttack, true);
        
        // Hit 애니메이션으로 전환 2번
        _stateContenxt.GetAnimator().SetInteger(NPCAnimHashID.Instance.MotionNum, 2);
        float duration = _stateContenxt.GetAnimator().GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);
        onFinished?.Invoke();
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
