using UnityEngine;


// 게임 상의 유저가 조작하는 플레이어를 관할하는 '추상'클래스입니다.
public abstract class Character : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackPower;

    public abstract void Move(Vector3 direction);
    public abstract void Attack(Vector3 targetPosition);

    public virtual void TakeDamage(float damage)
    {
    }

    public virtual void Die()
    {
    }
}
