using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int dungeonWidth = 16;
    public int dungeonHeight = 16;
    public int minRoomSize = 3;
    public int maxRoomSize = 6;

    [Header("Tile Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;

    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();

    public void GenerateDungeon()
    {
        BSPNode rootNode = new BSPNode(new RectInt(0, 0, dungeonWidth, dungeonHeight));
        List<RectInt> rooms = rootNode.Split(minRoomSize, maxRoomSize);

        foreach (RectInt room in rooms)
        {
            for (int x = room.xMin + 1; x < room.xMax - 1; x++)
            {
                for (int y = room.yMin + 1; y < room.yMax - 1; y++)
                {
                    floorPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int start = Vector2Int.RoundToInt(rooms[i].center);
            Vector2Int end = Vector2Int.RoundToInt(rooms[i + 1].center);

            CreateCorridor(start, end);
        }

        PaintTiles();
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int pos = start;

        while (pos.x != end.x)
        {
            floorPositions.Add(pos);
            pos.x += (end.x > pos.x) ? 1 : -1;
        }
        while (pos.y != end.y)
        {
            floorPositions.Add(pos);
            pos.y += (end.y > pos.y) ? 1 : -1;
        }

        floorPositions.Add(end);
    }

    public Tile GetTile(Vector2Int pos)
    {
        tiles.TryGetValue(pos, out var tile);
        return tile;
    }


    void PaintTiles()
    {
        foreach (var pos in floorPositions)
        {
            GameObject tileGO = Instantiate(floorPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, GameManager.Instance.dungeonWrapper);

            Tile tile = tileGO.GetComponent<Tile>();
            if (tile != null)
            {
                tiles[pos] = tile;
                tile.gridPosition = pos;
            }
        }


        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var pos in floorPositions)
        {
            foreach (var dir in new Vector2Int[] {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            })
            {
                Vector2Int wallPos = pos + dir;
                if (!floorPositions.Contains(wallPos))
                    wallPositions.Add(wallPos);
            }
        }

        foreach (var pos in wallPositions)
        {
            Instantiate(wallPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, GameManager.Instance.dungeonWrapper);
        }
    }

    public HashSet<Vector2Int> GetFloorPositions()
    {
        return new HashSet<Vector2Int>(floorPositions);
    }
}
