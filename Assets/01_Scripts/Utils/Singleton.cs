using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T) FindObjectOfType(typeof(T));

                Debug.Log($"Singleton {typeof(T)} Created");
            }
            return _instance;
        }
    }

    protected void OnDestroy()
    {
        Debug.Log($"Singloton Instance - {typeof(T)} destroyed");
    }
}
