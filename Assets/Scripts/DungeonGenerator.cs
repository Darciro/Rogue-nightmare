using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int dungeonWidth = 16;
    public int dungeonHeight = 16;
    public int minRoomSize = 3;
    public int maxRoomSize = 6;

    [Header("Tile Variants")]
    public GameObject[] floorPrefabs;

    [Header("Walls")]
    public GameObject wallBodyPrefab;
    public GameObject wallTopCapPrefab;

    [Header("Wall Variant Prefabs")]
    public GameObject wallHorizontalPrefab;
    public GameObject wallVerticalPrefab;
    public GameObject wallInnerCornerPrefab;
    public GameObject wallOuterCornerPrefab;
    public GameObject wallTopLayerPrefab;


    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();

    readonly Vector2Int[] cardinalDirs = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public void GenerateDungeon()
    {
        BSPNode rootNode = new BSPNode(new RectInt(0, 0, dungeonWidth, dungeonHeight));
        List<RectInt> rooms = rootNode.Split(minRoomSize, maxRoomSize);

        foreach (RectInt room in rooms)
            CarveRoom(room);

        for (int i = 0; i < rooms.Count - 1; i++)
            CreateCorridor(Vector2Int.RoundToInt(rooms[i].center), Vector2Int.RoundToInt(rooms[i + 1].center));

        PaintFloor();
        PaintTiles();
        // PaintWalls();
    }

    void CarveRoom(RectInt room)
    {
        for (int x = room.xMin + 1; x < room.xMax - 1; x++)
        {
            for (int y = room.yMin + 1; y < room.yMax - 1; y++)
            {
                floorPositions.Add(new Vector2Int(x, y));
            }
        }
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

    void PaintTiles()
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var pos in floorPositions)
        {
            // Check all four cardinal directions around each floor
            foreach (var dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = pos + dir;
                if (!floorPositions.Contains(neighbor))
                {
                    wallPositions.Add(neighbor); // neighbor is edge of a room/corridor -> wall
                }
            }

            // **Extra**: Add diagonal wall at room corners (outer corner gaps)
            // If this floor tile has no floor on, say, its top and right, then the top-right diagonal should be a wall.
            if (!floorPositions.Contains(pos + Vector2Int.up) && !floorPositions.Contains(pos + Vector2Int.right))
            {
                Vector2Int corner = pos + Vector2Int.up + Vector2Int.right;
                if (!floorPositions.Contains(corner))
                    wallPositions.Add(corner);
            }
            if (!floorPositions.Contains(pos + Vector2Int.up) && !floorPositions.Contains(pos + Vector2Int.left))
            {
                Vector2Int corner = pos + Vector2Int.up + Vector2Int.left;
                if (!floorPositions.Contains(corner))
                    wallPositions.Add(corner);
            }
            if (!floorPositions.Contains(pos + Vector2Int.down) && !floorPositions.Contains(pos + Vector2Int.right))
            {
                Vector2Int corner = pos + Vector2Int.down + Vector2Int.right;
                if (!floorPositions.Contains(corner))
                    wallPositions.Add(corner);
            }
            if (!floorPositions.Contains(pos + Vector2Int.down) && !floorPositions.Contains(pos + Vector2Int.left))
            {
                Vector2Int corner = pos + Vector2Int.down + Vector2Int.left;
                if (!floorPositions.Contains(corner))
                    wallPositions.Add(corner);
            }
        }

        // 3. Instantiate walls with the correct variant
        foreach (var pos in wallPositions)
        {
            // Skip if this position is actually a floor (just in case)
            if (floorPositions.Contains(pos)) continue;

            // Determine which neighbors are walls
            bool wallUp = wallPositions.Contains(pos + Vector2Int.up);
            bool wallDown = wallPositions.Contains(pos + Vector2Int.down);
            bool wallLeft = wallPositions.Contains(pos + Vector2Int.left);
            bool wallRight = wallPositions.Contains(pos + Vector2Int.right);

            // Determine if diagonals contain floor (for corner type check)
            bool floorNE = floorPositions.Contains(pos + Vector2Int.up + Vector2Int.right);
            bool floorNW = floorPositions.Contains(pos + Vector2Int.up + Vector2Int.left);
            bool floorSE = floorPositions.Contains(pos + Vector2Int.down + Vector2Int.right);
            bool floorSW = floorPositions.Contains(pos + Vector2Int.down + Vector2Int.left);

            GameObject prefabToUse = null;
            Quaternion rotation = Quaternion.identity;

            String debugText = "";

            // Corner cases – check perpendicular wall neighbors
            if (wallUp && wallRight && !wallDown && !wallLeft)
            {
                // Walls to the North and East of this tile (opening toward bottom-left)
                if (floorNE)
                {
                    prefabToUse = wallInnerCornerPrefab;   // inner corner (concave) 
                }
                else
                {
                    prefabToUse = wallOuterCornerPrefab;   // outer corner (convex)
                }
                rotation = Quaternion.Euler(0, 0, 0);  // default orientation (corner opening to bottom-left)
            }
            else if (wallUp && wallLeft && !wallDown && !wallRight)
            {
                // Walls to the North and West (opening toward bottom-right)
                debugText = "Walls to the North and West (opening toward bottom-right)";
                if (floorNW)
                    prefabToUse = wallInnerCornerPrefab;
                else
                    prefabToUse = wallOuterCornerPrefab;
                rotation = Quaternion.Euler(0, 0, 90);  // rotate 90° clockwise (corner opens to bottom-right)
            }
            else if (wallDown && wallRight && !wallUp && !wallLeft)
            {
                // Walls to the South and East (opening toward top-left)
                debugText = "Walls to the South and East (opening toward top-left)";
                if (floorSE)
                    prefabToUse = wallInnerCornerPrefab;
                else
                    prefabToUse = wallOuterCornerPrefab;
                rotation = Quaternion.Euler(0, 0, -90); // or 270° (corner opens to top-left)
            }
            else if (wallDown && wallLeft && !wallUp && !wallRight)
            {
                // Walls to the South and West (opening toward top-right)
                debugText = "Walls to the South and West (opening toward top-right)";
                if (floorSW)
                    prefabToUse = wallInnerCornerPrefab;
                else
                    prefabToUse = wallOuterCornerPrefab;
                rotation = Quaternion.Euler(0, 0, 180); // rotate 180° (corner opens to top-right)
            }
            // Straight edge cases
            else if ((wallLeft && wallRight) && !wallUp && !wallDown)
            {
                // Horizontal wall (neighbors on left and right)
                debugText = "Horizontal wall (neighbors on left and right)";
                prefabToUse = wallHorizontalPrefab;
                rotation = Quaternion.identity;
            }
            else if ((wallUp && wallDown) && !wallLeft && !wallRight)
            {
                // Vertical wall (neighbors above and below)
                bool floorOnRight = floorPositions.Contains(pos + Vector2Int.right);
                prefabToUse = wallVerticalPrefab;

                debugText = "Vertical wall (neighbors above and below)";
                if (floorOnRight)
                {
                    debugText += " - Vertical wall with FLOOR to the RIGHT";
                    rotation = Quaternion.Euler(0, 0, 180);
                }
                else
                {
                    debugText += " - Vertical wall (neighbors above and below)";
                    rotation = Quaternion.identity;
                }

                // prefabToUse = wallVerticalPrefab;
                // rotation = Quaternion.identity;
            }
            else
            {
                // Fallback: one-neighbor or isolated wall (use appropriate edge piece)
                debugText = "Fallback: one-neighbor or isolated wall (use appropriate edge piece)";
                if (wallLeft || wallRight)
                {
                    // Ends of a horizontal wall line
                    prefabToUse = wallHorizontalPrefab;
                    rotation = Quaternion.identity;
                }
                else if (wallUp || wallDown)
                {
                    // Ends of a vertical wall line
                    prefabToUse = wallVerticalPrefab;
                    rotation = Quaternion.identity;
                }
                else
                {
                    // Completely isolated wall (shouldn't happen often)
                    prefabToUse = wallHorizontalPrefab;
                }
            }

            // Instantiate the chosen wall prefab
            GameObject wallPiece = Instantiate(prefabToUse, new Vector3(pos.x, pos.y, 0), rotation, GameManager.Instance.dungeonWrapper);
            wallPiece.name = $"DebugObj ({debugText}, orinal name {wallPiece.name})";
            // 4. Add the visual-only top layer if this wall should have a tall appearance
            // We add the top piece if this wall tile has no wall directly above it.
            // (Also ensure we don't add a top if there's floor above and this wall is the bottom edge of a room – to avoid blocking that room's interior view.)
            bool floorAbove = floorPositions.Contains(pos + Vector2Int.up);
            if (!wallUp && !floorAbove)
            {
                Vector2Int topPos = pos + Vector2Int.up;
                // Place the top layer at the above grid position
                GameObject topPiece = Instantiate(wallTopLayerPrefab, new Vector3(topPos.x, topPos.y, 0), rotation, GameManager.Instance.dungeonWrapper);
                topPiece.name = $"DebugObj ({wallTopLayerPrefab}, orinal name {topPiece.name})";
                // Ensure the top piece renders above characters (set in prefab or via code)
                // e.g., topPiece.GetComponent<SpriteRenderer>().sortingLayerName = "WallsTop";
            }
        }
    }


    void PaintFloor()
    {
        foreach (var pos in floorPositions)
        {
            var prefab = floorPrefabs[UnityEngine.Random.Range(0, floorPrefabs.Length)];
            var tileGO = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, GameManager.Instance.dungeonWrapper);

            var tile = tileGO.GetComponent<Tile>();
            if (tile != null)
            {
                tiles[pos] = tile;
                tile.gridPosition = pos;
            }
        }
    }

    void PaintWalls()
    {
        var wallPositions = new HashSet<Vector2Int>();

        foreach (var floorPos in floorPositions)
        {
            foreach (var dir in cardinalDirs)
            {
                var potentialWallPos = floorPos + dir;
                if (!floorPositions.Contains(potentialWallPos))
                    wallPositions.Add(potentialWallPos);
            }
        }

        foreach (var wallPos in wallPositions)
        {
            if (IsSurroundedByWalls(wallPos)) continue;

            InstantiateWallWithCap(wallPos);
        }
    }

    bool IsSurroundedByWalls(Vector2Int wallPos)
    {
        foreach (var dir in cardinalDirs)
        {
            if (floorPositions.Contains(wallPos + dir))
                return false; // at least one adjacent floor tile
        }
        return true; // completely surrounded by walls (skip placing)
    }

    void InstantiateWallWithCap(Vector2Int pos)
    {
        Vector3 wallBodyPos = new Vector3(pos.x, pos.y, 0f);
        Instantiate(wallBodyPrefab, wallBodyPos, Quaternion.identity, GameManager.Instance.dungeonWrapper);

        // Place wall top cap if there is a floor below current wall position
        if (floorPositions.Contains(pos + Vector2Int.down))
        {
            Vector3 wallCapPos = new Vector3(pos.x, pos.y + 1, 0f);
            Instantiate(wallTopCapPrefab, wallCapPos, Quaternion.identity, GameManager.Instance.dungeonWrapper);
        }
    }

    public Tile GetTile(Vector2Int pos)
    {
        tiles.TryGetValue(pos, out var tile);
        return tile;
    }

    public HashSet<Vector2Int> GetFloorPositions()
    {
        return new HashSet<Vector2Int>(floorPositions);
    }
}
