using System;
using UnityEngine;

    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")] 
        [SerializeField] private float fieldOfView = 60f;
        
        private UnityEngine.Camera _camera;

        private void Start()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.fieldOfView = fieldOfView;
            
            transform.position = new Vector3(0f, 15f, -10f);
            transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        }
    }