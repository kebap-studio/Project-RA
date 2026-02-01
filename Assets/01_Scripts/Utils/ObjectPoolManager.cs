using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private Dictionary<int, object> _pools = new Dictionary<int, object>();
    private Dictionary<int, int> _activeObjects = new Dictionary<int, int>();

    public T Pop<T>(T prefab) where T : Component
    {
        // 프리팹의 key (실제 게임내의 개별의 인스턴스의 id가 아니다)
        int key = prefab.GetInstanceID();
        
        if (!_pools.ContainsKey(key))
        {
            _pools.Add(key, new ObjectPool<T>(
                createFunc: () => Instantiate(prefab), // 꺼낼때도 비어있으면 자동생성함
                actionOnGet: (obj) =>
                {
                    obj.gameObject.SetActive(true);
                    // 인스턴스의 id를 물고 있는다.
                    _activeObjects.Add(obj.GetInstanceID(), key);
                },
                actionOnRelease: (obj) => obj.gameObject.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj.gameObject), // push할때 maxsize보다 크면 삭제함.
                collectionCheck: true, 
                defaultCapacity: 10, 
                maxSize: 100
                ));
            
            Debug.Log($"[{prefab.name}] Create Prefab ObjectPool: {key}");
        }
        else
        {
            Debug.Log($"[{prefab.name}] Pop Prefab ObjectPool");
        }
        
        return ((ObjectPool<T>)_pools[prefab.GetInstanceID()]).Get();
    }

    public void Push<T>(T prefab) where T : Component
    {
        // 인스턴스의 id를 물고 있던거를 지운다
        int instanceID = prefab.GetInstanceID();
        int key = _activeObjects[instanceID];
        _activeObjects.Remove(instanceID);
        
        Debug.Log($"[{prefab.name}] Push Prefab ObjectPool: {key}");
        if (_pools.ContainsKey(key))
        {
            ((ObjectPool<T>)_pools[key]).Release(prefab);
            prefab.transform.SetParent(null, false);
        }
    }
}
