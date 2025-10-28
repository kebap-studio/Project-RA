using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomInfo
{
    public Vector3 position;
    public int width;
    public int height;
    public Transform roomTransform; // ???

    // 생성자로 처리하면 안되나?
    public RoomInfo(Vector3 position, int width, int height, Transform roomTransform)
    {
        this.position = position;
        this.width = width;
        this.height = height;
        this.roomTransform = roomTransform;
    }

    public Vector3 GetCenter()
    {
        return position + new Vector3(width * 0.5f, 0, height * 0.5f);
    }
}

public class DungeonConnector : MonoBehaviour
{
    [Header("References")]
    public RoomGenerator roomGenerator;
    public GridBlockGenerator corridorGenerator;

    [Header("Connection Settings")]
    public GameObject corridorFloorPrefab;
    public GameObject corridorWallPrefab;
    public float corridorWidth = 3f;

    [Header("Room Settings")]
    public int numberOfRooms = 4;
    public Vector2 roomSizeRange = new Vector2(8, 12);
    public float minRoomDistance = 15f;

    private List<RoomInfo> rooms = new List<RoomInfo>();

    [ContextMenu("Generate Connected Dungeon")]
    public void GenerateConnectedDungeon()
    {
        ClearExistingDungeon();
        GenerateRooms();
        ConnectRoomsWithCorridors();
    }

    private void ClearExistingDungeon()
    {
        rooms.Clear();

        if (roomGenerator != null)
            roomGenerator.DeleteAll();

        // 이게 되나?
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

    }

    private void GenerateRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            Vector3 roomPos = FindValidRoomPosition();
            int roomWidth = Random.Range((int)roomSizeRange.x, (int)roomSizeRange.y + 1);
            int roomHeight = Random.Range((int)roomSizeRange.x, (int)roomSizeRange.y + 1);

            Transform roomTransform = CreateRoom(roomPos, roomWidth, roomHeight);

            if (roomTransform != null)
            {
                RoomInfo newRoom = new RoomInfo(roomPos, roomWidth, roomHeight, roomTransform);
                rooms.Add(newRoom);
            }
        }
    }

    private Vector3 FindValidRoomPosition()
    {
        Vector3 position;
        int attempts = 0;
        const int maxAttempts = 100;

        do
        {
            // Range 범위를 수정해야 되지 않을까?
            position = new Vector3(
                Random.Range(-50f, 50f),
                0f,
                Random.Range(-50f, 50f));
            attempts++;
        } while (!isValidRoomPosition(position) && attempts < maxAttempts);

        return position;
    }

    private bool isValidRoomPosition(Vector3 position)
    {
        foreach (RoomInfo existingRoom in rooms)
        {
            float distance = Vector3.Distance(position, existingRoom.position);
            if (distance < minRoomDistance)
                return false;
        }
        return true;
    }

    private Transform CreateRoom(Vector3 position, int width, int height)
    {
        if (roomGenerator == null) return null;

        Vector3 originalOrigin = roomGenerator.originPos;
        int originalWidth = roomGenerator.roomWidth;
        int originalHeight = roomGenerator.roomHeight;

        roomGenerator.originPos = position;
        roomGenerator.roomWidth = width;
        roomGenerator.roomHeight = height;

        roomGenerator.GenerateRoom();

        Transform newRoomTransform = roomGenerator.GetLastGeneratedRoom();
        
        roomGenerator.originPos = originalOrigin;
        roomGenerator.roomWidth = originalWidth;
        roomGenerator.roomHeight = originalHeight;


        return newRoomTransform;
    }

    private void ConnectRoomsWithCorridors()
    {
        if (rooms.Count < 2) return;

        // MST
        List<RoomInfo> connectedRooms = new List<RoomInfo> { rooms[0] };
        List<RoomInfo> unconnectedRooms = new List<RoomInfo>(rooms);
        unconnectedRooms.RemoveAt(0);

        while (unconnectedRooms.Count > 0)
        {
            float shortestDistance = float.MaxValue;
            RoomInfo closestConnected = null;
            RoomInfo closestUnconnected = null;

            foreach (RoomInfo connected in connectedRooms)
            {
                foreach (RoomInfo unconnected in unconnectedRooms)
                {
                    float distance = Vector3.Distance(connected.GetCenter(), unconnected.GetCenter());
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestConnected = connected;
                        closestUnconnected = unconnected;
                    }
                }
            }

            if (closestConnected != null && closestUnconnected != null)
            {
                CreateCorridor(closestConnected.GetCenter(), closestUnconnected.GetCenter());
                connectedRooms.Add(closestUnconnected);
                unconnectedRooms.Remove(closestUnconnected);
            }
        }
    }

    private void CreateCorridor(Vector3 start, Vector3 end)
    {
        // ??????????? ㅇㅎ
        Vector3 corner = new Vector3(end.x, start.y, start.z);

        CreateCorridorSegment(start, corner);

        CreateCorridorSegment(corner, end);
    }


    private void CreateCorridorSegment(Vector3 start, Vector3 end)
    {
        if (Vector3.Distance(start, end) < 0.1f) return;

        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        for (float i = 0; i <= distance; i += 1f)
        {
            Vector3 position = start + direction * i;

            
            
            if (corridorFloorPrefab != null)
            {
                GameObject floor = Instantiate(corridorFloorPrefab, position, Quaternion.identity);
                floor.name = $"Corridor_Floor_{i}";
                floor.transform.SetParent(transform);
            }

            if (corridorWallPrefab != null)
            {
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
                float wallHeight = 1f;
                
                Vector3 leftWallPos = position + perpendicular * (corridorWidth * 0.5f);
                leftWallPos.y += wallHeight;
                
                GameObject leftWall = Instantiate(corridorWallPrefab, leftWallPos, Quaternion.identity);
                leftWall.name = $"Corridor_Wall_L_{i}";
                leftWall.transform.SetParent(transform);

                Vector3 rightWallPos = position - perpendicular * (corridorWidth * 0.5f);
                rightWallPos.y += wallHeight;
                GameObject rightWall = Instantiate(corridorWallPrefab, rightWallPos, Quaternion.identity);
                rightWall.name = $"Corridor_Wall_R_{i}";
                rightWall.transform.SetParent(transform);
            }
        }
    }
}
