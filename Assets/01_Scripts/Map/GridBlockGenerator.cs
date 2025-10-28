using UnityEngine;


public enum GridPattern
{
    Filled, // 전체 채우기
    Border, // 테두리만
    Checkered, // 체스판 패턴
    Cross, // 십자가 패턴
    Random // 랜덤 배치
}


public class GridBlockGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject[] blockPrefabs;
    public float[] spawnWeights;
    public int width = 20;
    public int height = 15;
    public Vector3 originPos = Vector3.zero;
    public float spacing = 1f;

    [Header("Pattern Settings")]
    public GridPattern pattern = GridPattern.Filled;
    [Range(0f, 1f)]
    public float randomDensity = 0.7f;

    [Header("Height Settings")]
    public bool useHeightVariation = false;
    public float baseHeight = -1f;
    public float maxHeightVariation = 3f;
    public float noiseScale = 0.1f;
    public AnimationCurve heightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    
    
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
                    GameObject prefabToUse = GetRandomPrefab();
                    if (prefabToUse != null)
                    {
                        float heightY = GetHeightAtPosition(x, z);
                        Vector3 position = originPos + new Vector3(x * spacing, heightY, z * spacing);
                        GameObject block = Instantiate(prefabToUse, position, Quaternion.identity);
                        block.name = $"{prefabToUse.name}_{x}_{z}";
                        block.transform.parent = transform;
                    }
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
                return (x == -width / 2 || x == width / 2 - 1 || z == -height / 2 || z == -height / 2 - 1);
            case GridPattern.Checkered:
                return (x + z) % 2 == 0;
            case GridPattern.Random:
                return Random.value < randomDensity;
            default:
                return true;
        }
    }

    private GameObject GetRandomPrefab()
    {
        if (blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogWarning("Block Prefabs array is empty.");
            return null;
        }

        if (blockPrefabs.Length == 1)
            return blockPrefabs[0];

        if (spawnWeights == null || spawnWeights.Length != blockPrefabs.Length)
            return blockPrefabs[Random.Range(0, blockPrefabs.Length)];

        float totalWeight = 0f;
        for (int i = 0; i < spawnWeights.Length; i++)
        {
            totalWeight += spawnWeights[i];
        }

        if (totalWeight <= 0f)
        {
            return blockPrefabs[Random.Range(0, blockPrefabs.Length)];
        }

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            currentWeight += spawnWeights[i];
            if (randomValue <= currentWeight)
                return blockPrefabs[i];
        }

        return blockPrefabs[blockPrefabs.Length - 1];
    }

    private float GetHeightAtPosition(int x, int z)
    {
        if (!useHeightVariation)
            return baseHeight;

        float noiseValue = Mathf.PerlinNoise(
            (x + width) * noiseScale,
            (z + height)  * noiseScale
        );

        float curveValue = heightCurve.Evaluate(noiseValue);
        
        return baseHeight + curveValue * maxHeightVariation;
    }
}
