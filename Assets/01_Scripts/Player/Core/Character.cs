using UnityEngine;
using System;


/// <summary>
/// 게임 상의 모든 캐릭터가 상속받는 추상 클래스
/// </summary>
public abstract class Character : MonoBehaviour
{
    [Header("Character Base Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth = 100f;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float attackPower = 10f;
    [SerializeField] protected bool isDead = false;

    // Events
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        // 시작 시 현재 체력을 최대 체력으로 설정
        currentHealth = maxHealth;
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// 캐릭터를 지정된 방향으로 이동시킵니다
    /// </summary>
    /// <param name="direction">이동할 방향 벡터</param>
    public abstract void Move(Vector3 direction);

    /// <summary>
    /// 지정된 위치를 공격합니다
    /// </summary>
    /// <param name="targetPosition">공격할 대상 위치</param>
    public abstract void Attack(Vector3 targetPosition);

    #endregion

    #region Health System

    /// <summary>
    /// 데미지를 받습니다
    /// </summary>
    /// <param name="damage">받을 데미지량</param>
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 캐릭터가 죽을 때 호출됩니다
    /// </summary>
    public virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDeath?.Invoke();

        Debug.Log($"[{GetType().Name}] {gameObject.name} has died");
    }

    /// <summary>
    /// 체력을 회복합니다
    /// </summary>
    /// <param name="amount">회복할 체력량</param>
    public virtual void Heal(float amount)
    {
        if (isDead) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (currentHealth != oldHealth)
        {
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    #endregion

    #region Public Getters

    /// <summary>
    /// 현재 체력을 반환합니다
    /// </summary>
    public float GetCurrentHealth() => currentHealth;

    /// <summary>
    /// 최대 체력을 반환합니다
    /// </summary>
    public float GetMaxHealth() => maxHealth;

    /// <summary>
    /// 체력 비율을 반환합니다 (0-1)
    /// </summary>
    public float GetHealthRatio() => maxHealth > 0 ? currentHealth / maxHealth : 0f;

    /// <summary>
    /// 캐릭터가 살아있는지 확인합니다
    /// </summary>
    public bool IsAlive() => currentHealth > 0;

    /// <summary>
    /// 공격력을 반환합니다
    /// </summary>
    public float GetAttackPower() => attackPower;

    /// <summary>
    /// 이동 속도를 반환합니다
    /// </summary>
    public float GetMoveSpeed() => moveSpeed;

    #endregion

    #region Protected Methods

    /// <summary>
    /// 최대 체력을 설정합니다
    /// </summary>
    protected void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// 이동 속도를 설정합니다
    /// </summary>
    protected void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0, value);
    }

    /// <summary>
    /// 공격력을 설정합니다
    /// </summary>
    protected void SetAttackPower(float value)
    {
        attackPower = Mathf.Max(0, value);
    }

    #endregion
}
