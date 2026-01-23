using UnityEngine;

public class TestDamageUI : MonoBehaviour
{
    [SerializeField] private Vector3 defaultPosition = new Vector3(3, 1, 65);
    [SerializeField] private DamageFontUI damagePrefab;
    
    void Start()
    {
        if (damagePrefab != null)
        {
            DamageFontUI obj = ObjectPoolManager.Instance.Pop(damagePrefab);
            if (obj != null)
            {
                obj.SetDamage(65000, defaultPosition);
            }
            
            DamageFontUI obj2 = ObjectPoolManager.Instance.Pop(damagePrefab);
            if (obj2 != null)
            {
                obj2.SetDamage(650010, new Vector3(5, 2, 65));
            }
            
            DamageFontUI obj3 = ObjectPoolManager.Instance.Pop(damagePrefab);
            if (obj3 != null)
            {
                obj3.SetDamage(650010, new Vector3(10, 1, 65));
            }
        }
    }
}
