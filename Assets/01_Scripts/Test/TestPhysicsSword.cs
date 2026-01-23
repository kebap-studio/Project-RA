using UnityEngine;

public class TestPhysicsSword : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 60f; // 초당 60도
    
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
