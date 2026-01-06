using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStateType 
{
    IDLE, 
    MOVE,
    CHASE,
    ATTACK,
    HITTED,
    DIE
}

// IState 인터페이스를 정의합니다.
public interface IState
{
    // init 메서드는 처음 초기화 될때 stateContext를 매개변수로 받는다. 
    public void init(AStateContext stateContext, Action func = null);

    public EStateType GetEStateType();

    // EnterState 메서드는 상태가 시작될 때 호출됩니다.
    // 이 메서드는 상태로 전환될 때 초기 설정을 수행하는 데 사용됩니다.
    public void EnterState();

    // UpdateState 메서드는 매 프레임마다 호출됩니다.
    // 이 메서드는 상태가 활성 상태일 때 실행되는 로직을 담당합니다.
    public IEnumerator UpdateState();

    // ExitState 메서드는 상태가 종료될 때 호출됩니다.
    // 이 메서드는 상태를 빠져나올 때 필요한 정리 작업을 수행하는 데 사용됩니다.
    public void ExitState();
}