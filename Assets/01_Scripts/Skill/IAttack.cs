using UnityEngine;

public interface IAttack
{
    public void Restart(int durationTime = -1);
    public void Stop();
}
