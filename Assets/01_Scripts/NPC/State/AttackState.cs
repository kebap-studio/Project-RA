using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AttackState : MonoBehaviour, IState
{
    [SerializeField] private float attackTime = 1.0f;
    [SerializeField] private CollisionAttack attackPrefab;

    private bool _isCollision = false;
    private float _attackStartTime;
    private AStateContext _stateContenxt;
    public Action onFinished;

    public void init(AStateContext stateContext, Action func = null)
    {
        _stateContenxt = stateContext;
        if (func != null)
            onFinished += func;
        
        // 시작지점은 어디쯤일까 일단. 20퍼센트 지난후 콜리전 체크해본다.
        _attackStartTime = attackTime * 0.2f;
    }

    public EStateType GetEStateType()
    {
        return EStateType.ATTACK;
    }

    public void EnterState()
    {
        if (_stateContenxt == null)
        {

            return;
        }
        _isCollision = false;
        StartCoroutine(UpdateState());
    }

    public IEnumerator UpdateState()
    {
        float time = 0.0f;
        while (time < attackTime)
        {
            if (time >= _attackStartTime && !_isCollision)
            {
                // 공격 애니메이션
                _stateContenxt.GetAnimator().SetBool(NPCAnimHashID.Instance.IsMoving, false);
                _stateContenxt.GetAnimator().SetBool(NPCAnimHashID.Instance.IsAttack, true);
                _stateContenxt.GetAnimator().SetInteger(NPCAnimHashID.Instance.MotionNum, 1);
                // attack 콜리전 생성
                _isCollision = true;
                CollisionAttack skillComponent = ObjectPoolManager.Instance.Pop(attackPrefab);
                if (skillComponent is IAttack attack)
                {
                    skillComponent.transform.SetParent(transform, false);
                    attack.Restart();
                }
            }
            // 시간 누적
            time += Time.deltaTime;
            yield return null;
        }
        _stateContenxt.GetAnimator().SetBool(NPCAnimHashID.Instance.IsMoving, true);
        _stateContenxt.GetAnimator().SetBool(NPCAnimHashID.Instance.IsAttack, false);
        onFinished?.Invoke();
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
