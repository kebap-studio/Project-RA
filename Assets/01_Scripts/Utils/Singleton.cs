using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Singloton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
            }
            return _instance;
        }
    }

    protected void OnDestroy()
    {
        Debug.Log($"Singloton Instance - {typeof(T)} destroyed");
    }
}
