using UnityEngine;

public class NPCAnimHashID : Singleton<NPCAnimHashID>
{
    public readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    public readonly int IsMoving = Animator.StringToHash("IsMoving");
    public readonly int IsAttack = Animator.StringToHash("IsAttack");
    public readonly int MotionNum = Animator.StringToHash("MotionNum");
}
