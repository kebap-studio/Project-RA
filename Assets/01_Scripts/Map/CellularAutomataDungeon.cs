using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Wall = 0,
    Floor = 1,
    SpecialRoom = 2
}

public class CellularAutomataDungeon : MonoBehaviour
{
    [Header("Grid Settings")]
    public int dungeonWidth = 80;
    public int dungeonHeight = 60;
    public float cellSize = 1f;
    public Vector3 dungeonCenter = Vector3.zero;

    [Header("Cellular Automata Settings")]
    [Range(0f, 1f)]
    public float initialWallProbability = 0.38f; 
    public int smoothingIterations = 10;
    public int wallThreshold = 4;

    [Header("Post Processing")]
    public int minCaveSize = 50;
    public bool connectDisconnectedAreas = true;

    [Header("References")]
    public GridBlockGenerator gridBlockGenerator;
    public RoomGenerator roomGenerator;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject corridorPrefab;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool visualizeGrid = false;

    private CellType[,] dungeonGrid;
    private List<List<Vector2Int>> caveRegions;

    [ContextMenu("Generate Cellular Automata Dungeon")]
    public void GenerateDungeon()
    {
        if (showDebugInfo) Debug.Log("=== Cellular Automata Dungeon Generation Started ===");

        InitializeGrid();
        ApplyCellularAutomata();
        ProcessCaveRegions();
        SpawnDungeonObjects();

        if (showDebugInfo) Debug.Log("=== Dungeon Generation Completed ===");
    }

    [ContextMenu("Clear Dungeon")]
    public void ClearDungeon()
    {
        ClearExistingObjects();
        dungeonGrid = null;
        caveRegions?.Clear();

        if (showDebugInfo) Debug.Log("Dungeon cleared!");
    }

    private void ClearExistingObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private Vector3 GridToWorldPosition(int gridX, int gridY)
    {
        return dungeonCenter + new Vector3((gridX - dungeonWidth * 0.5f) * cellSize,
            0f,
            (gridY - dungeonHeight * 0.5f) * cellSize);
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < dungeonWidth && y >= 0 && y < dungeonHeight;
    }

    private void OnDrawGizmos()
    {
        if (!visualizeGrid || dungeonGrid == null) return;

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3 worldPos = GridToWorldPosition(x, y);

                switch (dungeonGrid[x, y])
                {
                    case CellType.Wall:
                        Gizmos.color = Color.black;
                        break;
                    case CellType.Floor:
                        Gizmos.color = Color.white;
                        break;
                    case CellType.SpecialRoom:
                        Gizmos.color = Color.red;
                        break;
                }

                Gizmos.DrawCube(worldPos, Vector3.one * cellSize * 0.8f);
            }
        }
    }


    private void InitializeGrid()
    {
        if (showDebugInfo) Debug.Log($"Initializing grid: {dungeonWidth}x{dungeonHeight}");

        dungeonGrid = new CellType[dungeonWidth, dungeonHeight];

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (x == 0 || x == dungeonWidth - 1 || y == 0 || y == dungeonHeight - 1)
                    dungeonGrid[x, y] = CellType.Wall;
                else
                    dungeonGrid[x, y] = Random.value < initialWallProbability ? CellType.Wall : CellType.Floor;
            }
        }

        if (showDebugInfo)
        {
            int wallCount = 0;
            int floorCount = 0;

            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int y = 0; y < dungeonHeight; y++)
                {
                    if (dungeonGrid[x, y] == CellType.Wall) wallCount++;
                    else if (dungeonGrid[x, y] == CellType.Floor) floorCount++;
                }
            }
            Debug.Log($"Initial grid: {wallCount} walls, {floorCount} floors");
        }
    }

    private void ApplyCellularAutomata()
    {
        if (showDebugInfo) Debug.Log($"Applying Cellular Automata: {smoothingIterations} iterations");

        for (int iteration = 0; iteration < smoothingIterations; iteration++)
        {
            CellType[,] newGrid = new CellType[dungeonWidth, dungeonHeight];

            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int y = 0; y < dungeonHeight; y++)
                {
                    int wallCount = CountWallsAround(x, y);

                    if (wallCount > wallThreshold)
                        newGrid[x, y] = CellType.Wall;
                    else if (wallCount < wallThreshold)
                        newGrid[x, y] = CellType.Floor;
                    else
                        newGrid[x, y] = dungeonGrid[x, y];
                }
            }
            dungeonGrid = newGrid;

            if (showDebugInfo)
            {
                int wallCount = CountCellsOfType(CellType.Wall);
                int floorCount = CountCellsOfType(CellType.Floor);
                Debug.Log($"Iteration {iteration + 1}: {wallCount} walls, {floorCount} floors");
            }
        }
    }

    private int CountWallsAround(int centerX, int centerY)
    {
        int wallCount = 0;

        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                if (x < 0 || x >= dungeonWidth || y < 0 || y >= dungeonHeight)
                    wallCount++;
                else if (dungeonGrid[x, y] == CellType.Wall)
                    wallCount++;
            }
        }
        
        return wallCount;
    }

    private int CountCellsOfType(CellType type)
    {
        int count = 0;

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (dungeonGrid[x, y] == type)
                    count++;
            }
        }

        return count;
    }
    
    private void ProcessCaveRegions()
    {
        // TODO
    }

    private void SpawnDungeonObjects()
    {
        if (showDebugInfo) Debug.Log($"Spawning dungeon objects");
        
        ClearExistingObjects();

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3 worldPos = GridToWorldPosition(x, y);

                switch (dungeonGrid[x, y])
                {
                    case CellType.Floor:
                        if (floorPrefab != null)
                        {
                            GameObject floor = Instantiate(floorPrefab, worldPos, Quaternion.identity);
                            floor.transform.SetParent(transform);
                            floor.name = $"Floor_{x}_{y}";
                        }
                        break;
                    case CellType.Wall:
                        if (wallPrefab != null)
                        {
                            GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity);
                            wall.transform.SetParent(transform);
                            wall.name = $"Wall_{x}_{y}";
                        }
                        break;
                }
            }

            if (showDebugInfo)
            {
                int spawnedObjects = transform.childCount;
                Debug.Log($"Spawned {spawnedObjects} objects");
            }
        }
    }
}
