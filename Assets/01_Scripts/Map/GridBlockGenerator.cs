using UnityEngine;


public enum GridPattern
{
    Filled,      // 전체 채우기
    Border,      // 테두리만
    Checkered,   // 체스판 패턴
    Cross,       // 십자가 패턴
    Random       // 랜덤 배치
}


public class GridBlockGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject stonePrefab;
    public int width = 20;
    public int height = 15;
    public Vector3 originPos = Vector3.zero;
    public float spacing = 1f;

    [Header("Pattern Settings")]
    public GridPattern pattern = GridPattern.Filled;
    [Range(0f, 1f)]
    public float randomDensity = 0.7f;
    
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
            for (int z = -height / 2; z < height / 2; z++)
            {
                if (ShouldPlaceBlock(x, z))
                {
                    Vector3 position = originPos + new Vector3(x * spacing, -1, z * spacing);
                    GameObject block = Instantiate(stonePrefab, position, Quaternion.identity);
                    block.name = $"Stone_{x}_{z}";
                    block.transform.parent = transform;
                }
            }
        }
    }

    private bool ShouldPlaceBlock(int x, int z)
    {
        switch (pattern)
        {
            case GridPattern.Filled:
                return true;
            case GridPattern.Border:
                return (x == -width / 2 || x == width / 2 - 1 || z == -height / 2 || z ==-height / 2 - 1);
            case GridPattern.Checkered:
                return (x + z) % 2 == 0;
            case GridPattern.Random:
                return Random.value < randomDensity;
            default:
                return true;
        }
    }
}
