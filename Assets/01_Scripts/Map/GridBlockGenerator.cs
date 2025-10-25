using UnityEngine;

public class GridBlockGenerator : MonoBehaviour
{
    public GameObject stonePrefab;
    public int width = 10;
    public int depth = 10;
    public float spacing = 1f;
    
    [ContextMenu("Generate Grid")]
    void GenerateGrid()
    {
        // 기존 자식 삭제
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        // 새로 생성
        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int z = -depth / 2; z < depth / 2; z++)
            {
                Vector3 position = new Vector3(x * spacing, -1, z * spacing);
                GameObject block = Instantiate(stonePrefab, position, Quaternion.identity);
                block.name = $"Stone_{x}_{z}";
                block.transform.parent = transform;
            }
        }
    }
}
