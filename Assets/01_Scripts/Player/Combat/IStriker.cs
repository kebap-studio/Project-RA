using System;

// IStriker 인터페이스를 정의합니다.
public interface IStriker
{
    public void OnAttack();
    public void OnFinished();

    // 이거는 리턴값이 스킬정보? 면 좋을듯한데
    // public *** GetSkillInfo();
}
