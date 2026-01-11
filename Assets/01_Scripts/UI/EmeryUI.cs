using System;
using UnityEngine;

public class EmeryUI : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    // 머리 위 여유분(Y)과 살짝 뒤(Z)
    [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, -0.2f);

    private Transform _uiPosition;

    void Awake()
    {
        // 일단은 실행은 되도록
        Activate(enemy);
    }

    void LateUpdate()
    {
        transform.position = _uiPosition.position;
        transform.LookAt(transform.position - Camera.main.transform.forward, Camera.main.transform.up);
    }

    void Activate(GameObject target)
    {
        enemy = target;
        _uiPosition = target.transform.Find("UiPoint");
        if (_uiPosition != null)
        {
            transform.position = _uiPosition.position;
            transform.position += target.transform.forward * offset.z;
        }
    }

    void Deactivate()
    {
        enemy = null;
    }
}
