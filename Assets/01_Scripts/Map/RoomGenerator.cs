using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject Floor;
    public GameObject Wall;

    [Header("Size settings")]
    public int roomWidth = 10;
    public int roomHeight = 10;
    public float tileSize = 1f;

    [Header("Placement")]
    // Generate position offset
    public Vector3 originPos = Vector3.zero;

    // Room List
    private readonly List<Transform> _rooms = new List<Transform>();

    void Start()
    {
        GenerateRoom();
    }

    [ContextMenu("Generate Room")]
    public void GenerateRoom()
    {
        if (Floor == null || Wall == null)
        {
            Debug.LogError("[RoomGenerator] Floor/Wall prefab을 Inspector에 할당하세요.");
            return;
        }

        // Generate room object
        Vector3 GenPos = transform.position + originPos;
        string roomName = $"Room{_rooms.Count + 1}";
        Transform room = new GameObject(roomName).transform;
        room.SetParent(null, true);

        // Generate Floor
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                Vector3 pos = GenPos + new Vector3(x * tileSize, 0f, y * tileSize);
                Instantiate(Floor, pos, Quaternion.identity, room);
            }
        }

        // Generate Wall
        for (int x = -1; x <= roomWidth; x++)
        {
            for (int y = -1; y <= roomHeight; y++)
            {
                bool isBorder = (x == -1 || x == roomWidth || y == -1 || y == roomHeight);
                if (isBorder == false) continue;

                Vector3 pos = GenPos + new Vector3(x * tileSize, 0f, y * tileSize);
                Instantiate(Wall, pos, Quaternion.identity, room);
            }
        }

        // add room at RoomList
        _rooms.Add(room);
        Debug.Log($"[RoomGenerator] Generated: {roomName} @ {GenPos}");
    }

    [ContextMenu("Delete Last")]
    public void DeleteLast()
    {
        if (_rooms.Count <= 0)
        {
            Debug.Log("[RoomGenerator] Nothing to delete.");
            return;
        }

        Transform last = _rooms[_rooms.Count - 1];

#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(last.gameObject);
        else Destroy(last.gameObject);
#else
        Destroy(last.gameObject);
#endif

        _rooms.RemoveAt(_rooms.Count - 1);
        Debug.Log("[RoomGenerator] Deleted last room");
    }

    [ContextMenu("Delete All")]
    public void DeleteAll()
    {
        if (_rooms.Count <= 0)
        {
            Debug.Log("[RoomGenerator] Nothing to delete.");
            return;
        }

        for (int i = _rooms.Count - 1; i >= 0; i--)
        {
            Transform r = _rooms[i];
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(r.gameObject);
            else Destroy(r.gameObject);
#else
            Destroy(r.gameObject);
#endif
        }

        _rooms.Clear();
        Debug.Log("[RoomGenerator] Deleted all rooms");
    }

}