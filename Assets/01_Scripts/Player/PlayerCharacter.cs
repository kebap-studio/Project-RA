using UnityEngine;
using System.Collections.Generic;

public class PlayerCharacter : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int currentHp;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Inventory")]
    [SerializeField] private List<Item> itemList = new List<Item>();

    void Awake()
    {
        // 시작 시 체력 초기화
        currentHp = maxHp;
    }

    // 전투 관련 로직

    // 플레이어 체력 감소
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        Debug.Log($"플레이어 피해: {damage}, 현재 HP: {currentHp}");

        if (currentHp == 0)
        {
            Die();
        }
    }

    // 현재 HP 반환
    public int GetCurrentHp()
    {
        return currentHp;
    }

    // 아이템 관련 로직

    // 아이템 추가
    public void AddItem(Item item)
    {
        itemList.Add(item);
    }

    // 현재 아이템 리스트 반환
    public List<Item> GetItemList()
    {
        return itemList;
    }

    // 기타 스탯 getter
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetAttackRange()
    {
        return attackRange;
    }

    // 사망 처리 (임시)
    private void Die()
    {
        Debug.Log("플레이어 사망");
    }
}
