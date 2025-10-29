using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
        if (showDebugInfo) Debug.Log("Processing Cave Regions");

        caveRegions = new List<List<Vector2Int>>();
        bool[,] visited = new bool[dungeonWidth, dungeonHeight];

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (dungeonGrid[x, y] == CellType.Floor && !visited[x, y])
                {
                    List<Vector2Int> region = FloodFillRegion(x, y, visited);

                    if (region.Count < minCaveSize)
                    {
                        foreach (Vector2Int cell in region)
                        {
                            dungeonGrid[cell.x, cell.y] = CellType.Wall;
                        }

                        if (showDebugInfo) Debug.Log($"Removed small region of size {region.Count}");
                    }
                    else
                    {
                        caveRegions.Add(region);
                        if (showDebugInfo) Debug.Log($"Found valid region of size {region.Count}");
                    }
                }
            }
        }

        if (showDebugInfo) Debug.Log($"Total valid cave regions: {caveRegions.Count}");

        if (connectDisconnectedAreas && caveRegions.Count > 1) ConnectCaveRegions();
    }

    private List<Vector2Int> FloodFillRegion(int startX, int startY, bool[,] visited)
    {
        List<Vector2Int> region = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            region.Add(current);

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, };

            foreach (Vector2Int dir in directions)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;

                if (IsValidPosition(newX, newY) && !visited[newX, newY] && dungeonGrid[newX, newY] == CellType.Floor)
                {
                    visited[newX, newY] = true;
                    queue.Enqueue(new Vector2Int(newX, newY));
                }
            }
        }
        return region;
    }

    private void ConnectCaveRegions()
    {
        if (showDebugInfo) Debug.Log("Connecting cave regions with tunnels");

        List<Vector2Int> mainRegion = GetLargestRegion();

        for (int i = 0; i < caveRegions.Count; i++)
        {
            if (caveRegions[i] == mainRegion) continue;

            var (point1, point2) = GetClosestPointBetweenRegions(mainRegion, caveRegions[i]);
            CreateTunnel(point1, point2);

            if (showDebugInfo) Debug.Log($"Connected region {i} to main region");
        }
    }

    private List<Vector2Int> GetLargestRegion()
    {
        List<Vector2Int> largest = caveRegions[0];

        foreach (List<Vector2Int> region in caveRegions)
        {
            if (region.Count > largest.Count) largest = region;
        }

        return largest;
    }

    // Tolelom
    // XXX: 이러면 양쪽에서 두 번 탐색할 이유는 없지 않나?
    private (Vector2Int point1, Vector2Int point2) GetClosestPointBetweenRegions(List<Vector2Int> region1,
        List<Vector2Int> region2)
    {
        Vector2Int closest1 = region1[0];
        Vector2Int closest2 = region2[0];
        float minDistance = float.MaxValue;

        foreach (Vector2Int point1 in region1)
        {
            foreach (Vector2Int point2 in region2)
            {
                float distance = Vector2.Distance(point1, point2);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest1 = point1;
                    closest2 = point2;
                }
            }
        }

        return (closest1, closest2);
    }

    private void CreateTunnel(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        while (current.x != end.x)
        {
            dungeonGrid[current.x, current.y] = CellType.Floor;
            current.x += (current.x < end.x) ? 1 : -1;
        }

        while (current.y != end.y)
        {
            dungeonGrid[current.x, current.y] = CellType.Floor;
            current.y += (current.y < end.y) ? 1 : -1;
        }

        dungeonGrid[end.x, end.y] = CellType.Floor;
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
