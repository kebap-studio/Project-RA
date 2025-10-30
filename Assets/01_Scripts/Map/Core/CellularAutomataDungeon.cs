using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


/// <summary>
/// Cellular Automata 알고리즘을 사용한 프로시저럴 던전 생성기
/// </summary>
public class CellularAutomataDungeon : MonoBehaviour
{
    #region Enums

    public enum CellType
    {
        Wall = 0,
        Floor = 1,
        SpecialRoom = 2
    }

    public enum RoomType
    {
        Start,
        End,
    }

    #endregion


    #region Inspector Fields

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
    public int startRoomSize = 4;
    public int endRoomSize = 4;
    public float minDistanceBetweenRooms = 30f;

    [Header("Special Room Prefabs (Optional)")]
    public GameObject startRoomFloorPrefab;
    public GameObject endRoomFloorPrefab;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool visualizeGrid = false;

    #endregion


    #region Private Fields

    private CellType[,] _dungeonGrid;
    private List<List<Vector2Int>> _caveRegions;
    private Dictionary<Vector2Int, RoomType> _specialRoomPositions = new Dictionary<Vector2Int, RoomType>();

    #endregion

    #region Constants

    private static readonly Vector2Int[] FourDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
    };

    #endregion

    [ContextMenu("Generate Cellular Automata Dungeon")]
    public void GenerateDungeon()
    {
        if (showDebugInfo) Debug.Log("=== Cellular Automata Dungeon Generation Started ===");

        InitializeGrid();
        ApplyCellularAutomata();
        ProcessCaveRegions();

        if (generateSpecialRooms && _caveRegions != null && _caveRegions.Count > 0)
            GenerateSpecialRooms();

        SpawnDungeonObjects();

        if (showDebugInfo) Debug.Log("=== Dungeon Generation Completed ===");
    }

    [ContextMenu("Clear Dungeon")]
    public void ClearDungeon()
    {
        ClearExistingObjects();
        _dungeonGrid = null;
        _caveRegions?.Clear();
        _specialRoomPositions.Clear();

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
        if (!visualizeGrid || _dungeonGrid == null) return;

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3 worldPos = GridToWorldPosition(x, y);

                switch (_dungeonGrid[x, y])
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

        _dungeonGrid = new CellType[dungeonWidth, dungeonHeight];

        int wallCount = 0;
        int floorCount = 0;

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (x == 0 || x == dungeonWidth - 1 || y == 0 || y == dungeonHeight - 1)
                {
                    _dungeonGrid[x, y] = CellType.Wall;
                    wallCount++;
                }
                else
                {
                    if (Random.value < initialWallProbability)
                    {
                        _dungeonGrid[x, y] = CellType.Wall;
                        wallCount++;
                    }
                    else
                    {
                        _dungeonGrid[x, y] = CellType.Floor;
                        floorCount++;
                    }
                }
            }
        }

        if (showDebugInfo)
            Debug.Log($"Initial grid: {wallCount} walls, {floorCount} floors");
    }

    private void ApplyCellularAutomata()
    {
        if (showDebugInfo) Debug.Log($"Applying Cellular Automata: {smoothingIterations} iterations");

        for (int iteration = 0; iteration < smoothingIterations; iteration++)
        {
            CellType[,] newGrid = new CellType[dungeonWidth, dungeonHeight];
            int wallCount = 0, floorCount = 0;

            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int y = 0; y < dungeonHeight; y++)
                {
                    int wallCountAround = CountWallsAround(x, y);

                    if (wallCountAround > wallThreshold)
                    {
                        newGrid[x, y] = CellType.Wall;
                        wallCount++;
                    }
                    else if (wallCountAround < wallThreshold)
                    {
                        newGrid[x, y] = CellType.Floor;
                        floorCount++;
                    }
                    else
                    {
                        newGrid[x, y] = _dungeonGrid[x, y];
                        if (newGrid[x, y] == CellType.Wall) wallCount++;
                        else if (newGrid[x, y] == CellType.Floor) floorCount++;
                    }
                }
            }
            _dungeonGrid = newGrid;

            if (showDebugInfo)
                Debug.Log($"Iteration {iteration + 1}: {wallCount} walls, {floorCount} floors");
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
                else if (_dungeonGrid[x, y] == CellType.Wall)
                    wallCount++;
            }
        }

        return wallCount;
    }

    private void ProcessCaveRegions()
    {
        if (showDebugInfo) Debug.Log("Processing Cave Regions");

        _caveRegions = new List<List<Vector2Int>>();
        bool[,] visited = new bool[dungeonWidth, dungeonHeight];

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (_dungeonGrid[x, y] == CellType.Floor && !visited[x, y])
                {
                    List<Vector2Int> region = FloodFillRegion(x, y, visited);

                    if (region.Count < minCaveSize)
                    {
                        foreach (Vector2Int cell in region)
                        {
                            _dungeonGrid[cell.x, cell.y] = CellType.Wall;
                        }

                        if (showDebugInfo) Debug.Log($"Removed small region of size {region.Count}");
                    }
                    else
                    {
                        _caveRegions.Add(region);
                        if (showDebugInfo) Debug.Log($"Found valid region of size {region.Count}");
                    }
                }
            }
        }

        if (showDebugInfo) Debug.Log($"Total valid cave regions: {_caveRegions.Count}");

        if (connectDisconnectedAreas && _caveRegions.Count > 1) ConnectCaveRegions();
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

            foreach (Vector2Int dir in FourDirections)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;

                if (IsValidPosition(newX, newY) && !visited[newX, newY] && _dungeonGrid[newX, newY] == CellType.Floor)
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
        if (_caveRegions == null || _caveRegions.Count <= 1)
        {
            if (showDebugInfo) Debug.Log("Skipping region connection: insufficient regions");
            return;
        }

        if (showDebugInfo) Debug.Log("Connecting cave regions with tunnels");

        List<Vector2Int> mainRegion = GetLargestRegion();

        if (mainRegion.Count == 0)
        {
            Debug.LogError("Main region is empty! Cannot connect regions.");
            return;
        }

        for (int i = 0; i < _caveRegions.Count; i++)
        {
            if (_caveRegions[i] == mainRegion) continue;

            var (point1, point2) = GetClosestPointBetweenRegions(mainRegion, _caveRegions[i]);
            CreateTunnel(point1, point2);

            if (showDebugInfo) Debug.Log($"Connected region {i} to main region");
        }
    }

    private List<Vector2Int> GetLargestRegion()
    {
        if (_caveRegions == null || _caveRegions.Count == 0)
        {
            Debug.LogError("GetLargestRegion: No cave regions available");
            return new List<Vector2Int>();
        }

        List<Vector2Int> largest = _caveRegions[0];

        foreach (List<Vector2Int> region in _caveRegions)
        {
            if (region.Count > largest.Count) largest = region;
        }

        if (showDebugInfo)
            Debug.Log($"Largest region size: {largest.Count}");

        return largest;
    }

    private (Vector2Int point1, Vector2Int point2) GetClosestPointBetweenRegions(List<Vector2Int> region1,
        List<Vector2Int> region2)
    {
        List<Vector2Int> sample1 = SampleRegionPoints(region1, 20);
        List<Vector2Int> sample2 = SampleRegionPoints(region2, 20);

        Vector2Int closest1 = sample1[0];
        Vector2Int closest2 = sample2[0];
        float minDistance = float.MaxValue;

        foreach (Vector2Int point1 in sample1)
        {
            foreach (Vector2Int point2 in sample2)
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

    private List<Vector2Int> SampleRegionPoints(List<Vector2Int> region, int maxSamples)
    {
        if (region.Count <= maxSamples) return region;

        List<Vector2Int> samples = new List<Vector2Int>();
        int step = region.Count / maxSamples;

        for (int i = 0; i < region.Count; i += step)
        {
            samples.Add(region[i]);
            if (samples.Count >= maxSamples) break;
        }

        return samples;
    }

    private void CreateTunnel(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        while (current.x != end.x)
        {
            if (IsValidPosition(current.x, current.y))
                _dungeonGrid[current.x, current.y] = CellType.Floor;
            current.x += (current.x < end.x) ? 1 : -1;
        }

        while (current.y != end.y)
        {
            if (IsValidPosition(current.x, current.y))
                _dungeonGrid[current.x, current.y] = CellType.Floor;
            current.y += (current.y < end.y) ? 1 : -1;
        }

        if (IsValidPosition(current.x, current.y))
            _dungeonGrid[end.x, end.y] = CellType.Floor;
    }

    private void SpawnDungeonObjects()
    {
        if (showDebugInfo) Debug.Log($"Spawning dungeon objects");

        ClearExistingObjects();
        int totalSpawned = 0;

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                Vector3 worldPos = GridToWorldPosition(x, y);

                switch (_dungeonGrid[x, y])
                {
                    case CellType.Floor:
                        if (floorPrefab != null)
                        {
                            GameObject floor = Instantiate(floorPrefab, worldPos, Quaternion.identity);
                            floor.transform.SetParent(transform);
                            floor.name = $"Floor_{x}_{y}";
                            totalSpawned++;
                        }
                        break;
                    case CellType.Wall:
                        if (wallPrefab != null)
                        {
                            GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity);
                            wall.transform.SetParent(transform);
                            wall.name = $"Wall_{x}_{y}";
                            totalSpawned++;
                        }
                        break;
                    case CellType.SpecialRoom:
                        GameObject specialFloor = GetSpecialFloorPrefab(x, y);
                        if (specialFloor != null)
                        {
                            GameObject floor = Instantiate(specialFloor, worldPos, Quaternion.identity);
                            floor.transform.SetParent(transform);
                            floor.name = $"SpecialFloor_{x}_{y}";
                            totalSpawned++;
                        }
                        break;
                }
            }
        }
        if (showDebugInfo)
            Debug.Log($"Spawned {totalSpawned} objects");
    }

    private GameObject GetSpecialFloorPrefab(int x, int y)
    {
        // XXX
        // 임시로 기본 바닥으로 설정 
        // 추후에 확장 예정
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

        (Vector2Int startRoomPos, Vector2Int endRoomPos) = FindStartAndEndInEntireDungeon();

        if (startRoomPos == Vector2Int.zero && endRoomPos == Vector2Int.zero)
        {
            Debug.LogWarning("Could not find suitable positions for Start and End rooms!");
            return;
        }

        CreateSpecialRoom(startRoomPos, RoomType.Start);
        CreateSpecialRoom(endRoomPos, RoomType.End);

        if (showDebugInfo)
        {
            float actualDistance = Vector2.Distance(startRoomPos, endRoomPos) * cellSize;
            Debug.Log($"Start room at {startRoomPos}, End room at {endRoomPos}, Distance: {actualDistance:F1}");
        }
    }

    private (Vector2Int startPos, Vector2Int endPos) FindStartAndEndInEntireDungeon()
    {
        List<Vector2Int> allFloorTiles = new List<Vector2Int>();

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                if (_dungeonGrid[x, y] == CellType.Floor)
                {
                    allFloorTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        if (allFloorTiles.Count < 2)
        {
            Debug.LogWarning("Not enough floor tiles for room placement");
            return (Vector2Int.zero, Vector2Int.zero);
        }

        int maxRoomSize = Mathf.Max(startRoomSize, endRoomSize);
        int safeMargin = maxRoomSize / 2 + 1;

        List<Vector2Int> safeFloorTiles = new List<Vector2Int>();
        foreach (Vector2Int tile in allFloorTiles)
        {
            if (tile.x >= safeMargin && tile.x < dungeonWidth - safeMargin &&
                tile.y >= safeMargin && tile.y < dungeonHeight - safeMargin)
                safeFloorTiles.Add(tile);
        }

        if (safeFloorTiles.Count < 2)
        {
            Debug.LogWarning("Not enough safe floor tiles for room placement");
            return (Vector2Int.zero, Vector2Int.zero);
        }

        Vector2Int bestStart = safeFloorTiles[0];
        Vector2Int bestEnd = safeFloorTiles[0];
        float maxDistance = 0f;

        for (int i = 0; i < safeFloorTiles.Count; i++)
        {
            for (int j = i + 1; j < safeFloorTiles.Count; j++)
            {
                float distance = Vector2.Distance(safeFloorTiles[i], safeFloorTiles[j]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bestStart = safeFloorTiles[i];
                    bestEnd = safeFloorTiles[j];
                }
            }
        }

        if (maxDistance * cellSize < minDistanceBetweenRooms)
        {
            if (showDebugInfo)
                Debug.LogWarning($"Best distance {maxDistance * cellSize} < required {minDistanceBetweenRooms}");
        }

        if (showDebugInfo)
        {
            Debug.Log($"Found {allFloorTiles.Count} total floor tiles, {safeFloorTiles.Count} safe tiles");
            Debug.Log(
                $"Selected global positions: Start={bestStart}, End={bestEnd}, Distance={maxDistance * cellSize:F1}");
        }

        return (bestStart, bestEnd);
    }

    private Vector2Int GetRegionEdgePosition(List<Vector2Int> region, bool preferLeft)
    {
        if (region.Count == 0) return Vector2Int.zero;

        // 방 크기 고려한 안전 마진 계산
        int maxRoomSize = Mathf.Max(startRoomSize, endRoomSize);
        int safeMargin = maxRoomSize / 2 + 1;

        Vector2Int edge = Vector2Int.zero;
        bool foundSafeEdge = false;

        if (preferLeft)
        {
            // 가장 왼쪽이면서 안전한 점 찾기
            int safeLeftX = int.MaxValue;

            foreach (Vector2Int point in region)
            {
                // 안전 영역 내에 있는지 체크
                bool isSafe = point.x >= safeMargin && point.x < dungeonWidth - safeMargin &&
                              point.y >= safeMargin && point.y < dungeonHeight - safeMargin;

                if (isSafe && point.x < safeLeftX)
                {
                    safeLeftX = point.x;
                    edge = point;
                    foundSafeEdge = true;
                }
            }
        }
        else
        {
            // 가장 오른쪽이면서 안전한 점 찾기
            int safeRightX = int.MinValue;

            foreach (Vector2Int point in region)
            {
                bool isSafe = point.x >= safeMargin && point.x < dungeonWidth - safeMargin &&
                              point.y >= safeMargin && point.y < dungeonHeight - safeMargin;

                if (isSafe && point.x > safeRightX)
                {
                    safeRightX = point.x;
                    edge = point;
                    foundSafeEdge = true;
                }
            }
        }

        // 안전한 가장자리를 못 찾으면 중앙으로 폴백
        if (!foundSafeEdge)
        {
            if (showDebugInfo)
                Debug.LogWarning($"No safe edge position found, using region center instead");
            edge = GetRegionCenter(region);

            // 중앙도 안전하지 않다면 안전 영역으로 클램프
            edge.x = Mathf.Clamp(edge.x, safeMargin, dungeonWidth - safeMargin - 1);
            edge.y = Mathf.Clamp(edge.y, safeMargin, dungeonHeight - safeMargin - 1);
        }

        return edge;
    }

    private (Vector2Int startPos, Vector2Int endPos) FindFallbackPositions()
    {
        List<List<Vector2Int>> sortedRegions = new List<List<Vector2Int>>(_caveRegions);
        sortedRegions.Sort((a, b) => b.Count.CompareTo(a.Count));

        // 🔧 수정: 경계 우선, 안전 체크 포함
        Vector2Int startPos = GetRegionEdgePosition(sortedRegions[0], true); // 안전한 왼쪽 끝
        Vector2Int endPos = sortedRegions.Count > 1
            ? GetRegionEdgePosition(sortedRegions[1], false) // 안전한 오른쪽 끝
            : GetRegionEdgePosition(sortedRegions[0], false); // 같은 영역의 안전한 오른쪽 끝

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

        _specialRoomPositions[center] = roomType;

        for (int x = center.x - halfSize; x <= center.x + halfSize; x++)
        {
            for (int y = center.y - halfSize; y <= center.y + halfSize; y++)
            {
                if (IsValidPosition(x, y))
                    _dungeonGrid[x, y] = CellType.SpecialRoom;
            }
        }
    }

    private Color GetSpecialRoomColor(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        foreach (var kvp in _specialRoomPositions)
        {
            float distance = Vector2Int.Distance(pos, kvp.Key);
            if (distance <= Mathf.Max(startRoomSize, endRoomSize) / 2f)
                return kvp.Value == RoomType.Start ? Color.green : Color.red;
        }

        return Color.yellow;
    }
}
