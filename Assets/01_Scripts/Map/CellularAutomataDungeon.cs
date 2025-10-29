using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
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

    [Header("Special Rooms")]
    public bool generateSpecialRooms = true;
    public int startRoomSize = 5;
    public int endRoomSize = 5;
    public float minDistanceBetweenRooms = 30f;

    [Header("Special Room Prefabs (Optional)")]
    public GameObject startRoomFloorPrefab;
    public GameObject endRoomFloorPrefab; // bossRoomFloorPrefab → endRoomFloorPrefab로 변경

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool visualizeGrid = false;

    private CellType[,] dungeonGrid;
    private List<List<Vector2Int>> caveRegions;
    private Dictionary<Vector2Int, RoomType> specialRoomPositions = new Dictionary<Vector2Int, RoomType>();

    private enum RoomType
    {
        Start,
        End,
    }


    [ContextMenu("Generate Cellular Automata Dungeon")]
    public void GenerateDungeon()
    {
        if (showDebugInfo) Debug.Log("=== Cellular Automata Dungeon Generation Started ===");

        InitializeGrid();
        ApplyCellularAutomata();
        ProcessCaveRegions();

        if (generateSpecialRooms && caveRegions != null && caveRegions.Count > 0)
            GenerateSpecialRooms();

        SpawnDungeonObjects();

        if (showDebugInfo) Debug.Log("=== Dungeon Generation Completed ===");
    }

    [ContextMenu("Clear Dungeon")]
    public void ClearDungeon()
    {
        ClearExistingObjects();
        dungeonGrid = null;
        caveRegions?.Clear();
        specialRoomPositions.Clear();

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
                        Gizmos.color = GetSpecialRoomColor(x, y);
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
                    case CellType.SpecialRoom:
                        GameObject specialFloor = GetSpecialFloorPrefab(x, y);
                        if (specialFloor != null)
                        {
                            GameObject floor = Instantiate(specialFloor, worldPos, Quaternion.identity);
                            floor.transform.SetParent(transform);
                            floor.name = $"SpecialFloor_{x}_{y}";
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

    private GameObject GetSpecialFloorPrefab(int x, int y)
    {
        // XXX
        // 임시로 기본 바닥으로 설정 
        return floorPrefab;
    }

    private void GenerateSpecialRooms()
    {
        if (showDebugInfo) Debug.Log($"Generating special rooms (Start & End)");

        if (roomGenerator == null)
        {
            Debug.LogWarning("RoomGenerator is null");
            return;
        }

        if (caveRegions.Count == 0)
        {
            Debug.LogWarning("No cave regions found!");
            return;
        }

        Vector2Int startPos, endPos;

        if (caveRegions.Count == 1)
        {
            // 단일 영역에서 시작방과 끝방 배치
            (startPos, endPos) = FindStartAndEndInSingleRegion(caveRegions[0]);
        }
        else
        {
            (startPos, endPos) = FindStartAndEndPositions();

        }

        if (startPos == Vector2Int.zero && endPos == Vector2Int.zero)
        {
            Debug.LogWarning("Could not find suitable positions for Start and End rooms!");
            return;
        }

        CreateSpecialRoom(startPos, RoomType.Start);
        CreateSpecialRoom(endPos, RoomType.End);

        if (showDebugInfo)
        {
            float actualDistance = Vector2.Distance(startPos, endPos) * cellSize;
            Debug.Log($"Start room at {startPos}, End room at {endPos}, Distance: {actualDistance:F1}");
        }
    }

    private (Vector2Int startPos, Vector2Int endPos) FindStartAndEndInSingleRegion(List<Vector2Int> region)
    {
        if (region.Count < 200)
        {
            Debug.LogWarning("Single region too small for both rooms");
            return (Vector2Int.zero, Vector2Int.zero);
        }

        // 영역의 경계 계산
        Vector2Int minBound = region[0];
        Vector2Int maxBound = region[0];

        foreach (Vector2Int point in region)
        {
            if (point.x < minBound.x) minBound.x = point.x;
            if (point.y < minBound.y) minBound.y = point.y;
            if (point.x > maxBound.x) maxBound.x = point.x;
            if (point.y > maxBound.y) maxBound.y = point.y;
        }

        // 방 크기를 고려한 안전 영역 계산
        int maxRoomSize = Mathf.Max(startRoomSize, endRoomSize);
        int safeMargin = maxRoomSize / 2 + 1; // 방 반 크기 + 여유분

        Vector2Int safeMinBound = new Vector2Int(minBound.x + safeMargin, minBound.y + safeMargin);
        Vector2Int safeMaxBound = new Vector2Int(maxBound.x - safeMargin, maxBound.y - safeMargin);

        // 추가로 맵 경계도 고려
        safeMinBound.x = Mathf.Max(safeMinBound.x, safeMargin);
        safeMinBound.y = Mathf.Max(safeMinBound.y, safeMargin);
        safeMaxBound.x = Mathf.Min(safeMaxBound.x, dungeonWidth - safeMargin);
        safeMaxBound.y = Mathf.Min(safeMaxBound.y, dungeonHeight - safeMargin);

        // 안전 영역이 너무 작으면 포기
        if (safeMaxBound.x <= safeMinBound.x || safeMaxBound.y <= safeMinBound.y)
        {
            Debug.LogWarning("Region too small for safe room placement");
            return (Vector2Int.zero, Vector2Int.zero);
        }

        // 안전 영역 내에서 가장 먼 두 점 찾기
        Vector2Int bestStart = safeMinBound;
        Vector2Int bestEnd = safeMaxBound;
        float maxDistance = 0f;

        // 영역 내에서 실제로 Floor인 지점들 중에서 선택
        List<Vector2Int> safePoints = new List<Vector2Int>();
        foreach (Vector2Int point in region)
        {
            if (point.x >= safeMinBound.x && point.x <= safeMaxBound.x &&
                point.y >= safeMinBound.y && point.y <= safeMaxBound.y)
            {
                safePoints.Add(point);
            }
        }

        if (safePoints.Count < 2)
        {
            Debug.LogWarning("Not enough safe points for room placement");
            return (Vector2Int.zero, Vector2Int.zero);
        }

        // 가장 먼 두 점 찾기 (브루트 포스 - 작은 리스트이므로 괜찮음)
        for (int i = 0; i < safePoints.Count; i++)
        {
            for (int j = i + 1; j < safePoints.Count; j++)
            {
                float distance = Vector2.Distance(safePoints[i], safePoints[j]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bestStart = safePoints[i];
                    bestEnd = safePoints[j];
                }
            }
        }

        // 최소 거리 체크
        if (maxDistance * cellSize < minDistanceBetweenRooms)
        {
            if (showDebugInfo)
                Debug.LogWarning(
                    $"Safe points too close. Distance: {maxDistance * cellSize}, Required: {minDistanceBetweenRooms}");

            // 그래도 최대한 떨어뜨려서 배치
            Vector2Int regionCenter = GetRegionCenter(region);
            Vector2Int safeOffset = new Vector2Int(
                Mathf.Min((int)(minDistanceBetweenRooms / cellSize / 2), (safeMaxBound.x - safeMinBound.x) / 2),
                0
            );

            bestStart = new Vector2Int(
                Mathf.Clamp(regionCenter.x - safeOffset.x, safeMinBound.x, safeMaxBound.x),
                Mathf.Clamp(regionCenter.y, safeMinBound.y, safeMaxBound.y)
            );

            bestEnd = new Vector2Int(
                Mathf.Clamp(regionCenter.x + safeOffset.x, safeMinBound.x, safeMaxBound.x),
                Mathf.Clamp(regionCenter.y, safeMinBound.y, safeMaxBound.y)
            );
        }

        if (showDebugInfo)
        {
            Debug.Log($"Safe bounds: {safeMinBound} to {safeMaxBound}");
            Debug.Log($"Selected positions: Start={bestStart}, End={bestEnd}");
        }

        return (bestStart, bestEnd);
    }

    private (Vector2Int startPos, Vector2Int endPos) FindStartAndEndPositions()
    {
        Vector2Int bestStartPos = Vector2Int.zero;
        Vector2Int bestEndPos = Vector2Int.zero;
        float maxDistance = 0f;

        for (int i = 0; i < caveRegions.Count; i++)
        {
            for (int j = i + 1; j < caveRegions.Count; j++)
            {
                Vector2Int pos1 = GetRegionCenter(caveRegions[i]);
                Vector2Int pos2 = GetRegionCenter(caveRegions[j]);

                float distance = Vector2.Distance(pos1, pos2) * cellSize;

                if (distance >= minDistanceBetweenRooms && distance > maxDistance)
                {
                    maxDistance = distance;

                    if (caveRegions[i].Count >= caveRegions[j].Count)
                    {
                        bestStartPos = pos1;
                        bestEndPos = pos2;
                    }
                    else
                    {
                        bestStartPos = pos2;
                        bestEndPos = pos1;
                    }
                }
            }
        }

        if (maxDistance == 0f)
        {
            if (showDebugInfo)
                Debug.LogWarning(
                    $"No regions found with minimum distance {minDistanceBetweenRooms}. Using fallback logic.");
            return FindFallbackPositions();
        }

        return (bestStartPos, bestEndPos);
    }

    private (Vector2Int startPos, Vector2Int endPos) FindFallbackPositions()
    {
        List<List<Vector2Int>> sortedRegions = new List<List<Vector2Int>>(caveRegions);
        sortedRegions.Sort((a, b) => b.Count.CompareTo(a.Count));

        Vector2Int startPos = GetRegionCenter(sortedRegions[0]);
        Vector2Int endPos = sortedRegions.Count > 1
            ? GetRegionCenter(sortedRegions[1])
            : GetRegionCenter(sortedRegions[0]);

        return (startPos, endPos);
    }

    // tolelom
    // XXX: ???
    private Vector2Int GetRegionCenter(List<Vector2Int> region)
    {
        if (region.Count == 0) return Vector2Int.zero;

        int totalX = 0;
        int totalY = 0;

        foreach (Vector2Int point in region)
        {
            totalX += point.x;
            totalY += point.y;
        }

        return new Vector2Int(totalX / region.Count, totalY / region.Count);
    }

    private void CreateSpecialRoom(Vector2Int gridPos, RoomType roomType)
    {
        Vector3 worldPos = GridToWorldPosition(gridPos.x, gridPos.y);

        int roomSize = GetRoomSizeForType(roomType);

        Vector3 originalOrigin = roomGenerator.originPos;
        int originalWidth = roomGenerator.roomWidth;
        int originalHeight = roomGenerator.roomHeight;

        roomGenerator.originPos = worldPos;
        roomGenerator.roomWidth = roomSize;
        roomGenerator.roomHeight = roomSize;

        roomGenerator.GenerateRoom();

        MarkSpecialRoomInGrid(gridPos, roomSize, roomType);

        roomGenerator.originPos = originalOrigin;
        roomGenerator.roomWidth = originalWidth;
        roomGenerator.roomHeight = originalHeight;

        if (showDebugInfo)
            Debug.Log($"Created {roomType} room at {worldPos} with size {roomSize}x{roomSize}");
    }

    private int GetRoomSizeForType(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Start:
                return startRoomSize;
            case RoomType.End:
                return endRoomSize;
            default:
                return 6;
        }
    }

    private void MarkSpecialRoomInGrid(Vector2Int center, int roomSize, RoomType roomType)
    {
        int halfSize = roomSize / 2;

        specialRoomPositions[center] = roomType;

        for (int x = center.x - halfSize; x <= center.x + halfSize; x++)
        {
            for (int y = center.y - halfSize; y <= center.y + halfSize; y++)
            {
                if (IsValidPosition(x, y))
                    dungeonGrid[x, y] = CellType.SpecialRoom;
            }
        }
    }

    private Color GetSpecialRoomColor(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        foreach (var kvp in specialRoomPositions)
        {
            float distance = Vector2Int.Distance(pos, kvp.Key);
            if (distance <= Mathf.Max(startRoomSize, endRoomSize) / 2f)
                return kvp.Value == RoomType.Start ? Color.green : Color.red;
        }

        return Color.yellow;
    }
}
