using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MoveState : MonoBehaviour, IState
{
    private AStateContext _stateContenxt;
    public void init(AStateContext stateContext, Action func = null)
    {
        _stateContenxt = stateContext;
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
                Vector2 size = new Vector2(10, 10);
                nextPoint = GetRandomPointInBox(curPoint, size);
                
                Quaternion newRotation = Quaternion.LookRotation(nextPoint);
                transform.rotation = newRotation;
            }

            Vector3 newPoint = Vector3.MoveTowards(curPoint, nextPoint, _stateContenxt._current.GetMoveSpeed() * Time.deltaTime);
            _stateContenxt._current.transform.position = newPoint;
            
            yield return null;
        }
    }

    public void ExitState()
    {
        StopCoroutine(UpdateState());
    }

    Vector3 GetRandomPointInBox(Vector3 center, Vector2 size)
    {
        // 가로(X), 세로(Z) 범위를 각각 랜덤으로 구함
        float randomX = UnityEngine.Random.Range(-size.x / 2, size.x / 2);
        float randomZ = UnityEngine.Random.Range(-size.y / 2, size.y / 2);

        // 높이(Y)는 0으로 고정
        Vector3 randomPos = new Vector3(randomX, 0, randomZ);

        return center + randomPos;
    }
}
