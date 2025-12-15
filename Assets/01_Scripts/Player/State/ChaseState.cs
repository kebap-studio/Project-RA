using System.Collections;
using UnityEngine;
using System;

public class ChaseState : MonoBehaviour, IState
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
            
            if (Vector3.Distance(playerPosition, _stateContenxt._current.transform.position) < 4.0f)
            {
                onFinished?.Invoke();
                yield break;
            }
            else
            {
                Quaternion newRotation = Quaternion.LookRotation(playerPosition);
                _stateContenxt._current.transform.rotation = newRotation;

                Vector3 curPoint = _stateContenxt._current.transform.position;
                Vector3 newPoint = Vector3.MoveTowards(curPoint, playerPosition, _stateContenxt._current.GetMoveSpeed() * Time.deltaTime);
                _stateContenxt._current.transform.position = newPoint;
            }

            yield return null;
        }
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }
}
