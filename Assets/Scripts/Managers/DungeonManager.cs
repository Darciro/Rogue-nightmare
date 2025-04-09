using RogueNightmare.Core;

namespace RogueNightmare.Dungeon
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class DungeonManager : MonoBehaviour
    {
        [Header("Dungeon Dimensions")]
        [SerializeField] private int dungeonWidth = 16;
        [SerializeField] private int dungeonHeight = 16;
        [SerializeField] private int minRoomSize = 3;
        [SerializeField] private int maxRoomSize = 6;

        public int DungeonWidth => dungeonWidth;
        public int DungeonHeight => dungeonHeight;

        [Header("Floor Tiles")]
        [SerializeField] private GameObject[] floorPrefabs;

        [Header("Wall Prefabs")]
        [SerializeField] private GameObject wallHorizontalPrefab;
        [SerializeField] private GameObject wallVerticalPrefab;
        [SerializeField] private GameObject wallInnerCornerPrefab;
        [SerializeField] private GameObject wallOuterCornerPrefab;

        // Internal data structures for floor and tiles
        private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        private readonly Vector2Int[] cardinalDirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        /// <summary>Generates the dungeon layout and instantiates floor and wall tiles.</summary>
        public void GenerateDungeon()
        {
            // 1. Use BSP to partition the dungeon area into rooms
            BSPNode rootNode = new BSPNode(new RectInt(0, 0, dungeonWidth, dungeonHeight));
            List<RectInt> rooms = rootNode.Split(minRoomSize, maxRoomSize);
            // 2. Carve out rooms and corridors in the floorPositions set
            foreach (RectInt room in rooms)
            {
                CarveRoom(room);
            }
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                CreateCorridor(Vector2Int.RoundToInt(rooms[i].center),
                               Vector2Int.RoundToInt(rooms[i + 1].center));
            }
            // 3. Instantiate floor tile GameObjects for all floor positions
            PaintFloor();
            // 4. Instantiate walls around floor edges using appropriate prefabs
            PaintWallsAndCorners();
        }

        // Carve a rectangular room area into the floorPositions set
        private void CarveRoom(RectInt roomArea)
        {
            for (int x = roomArea.xMin + 1; x < roomArea.xMax - 1; x++)
            {
                for (int y = roomArea.yMin + 1; y < roomArea.yMax - 1; y++)
                {
                    floorPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        // Carve a straight corridor between two points in the dungeon
        private void CreateCorridor(Vector2Int start, Vector2Int end)
        {
            Vector2Int pos = start;
            // carve horizontal path
            while (pos.x != end.x)
            {
                floorPositions.Add(pos);
                pos.x += (end.x > pos.x) ? 1 : -1;
            }
            // carve vertical path
            while (pos.y != end.y)
            {
                floorPositions.Add(pos);
                pos.y += (end.y > pos.y) ? 1 : -1;
            }
            floorPositions.Add(end);
        }

        // Instantiate floor tile GameObjects at each floor position
        private void PaintFloor()
        {
            foreach (var pos in floorPositions)
            {
                GameObject prefab = floorPrefabs[UnityEngine.Random.Range(0, floorPrefabs.Length)];
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0f);
                GameObject tileGO = Instantiate(prefab, worldPos, Quaternion.identity, GameManager.Instance.DungeonParent);
                Tile tileComponent = tileGO.GetComponent<Tile>();
                if (tileComponent != null)
                {
                    tiles[pos] = tileComponent;
                    tileComponent.gridPosition = pos;
                }
            }
        }

        // Instantiate wall prefabs around floor edges and at corners
        private void PaintWallsAndCorners()
        {
            String debugText = "";
            HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
            // Determine all grid positions adjacent to floors that need walls
            foreach (var floorPos in floorPositions)
            {
                foreach (var dir in cardinalDirs)
                {
                    Vector2Int neighbor = floorPos + dir;
                    if (!floorPositions.Contains(neighbor))
                        wallPositions.Add(neighbor);
                }
                // Check diagonal gaps (outer corners) and fill with walls
                if (!floorPositions.Contains(floorPos + Vector2Int.up) &&
                    !floorPositions.Contains(floorPos + Vector2Int.right))
                {
                    Vector2Int corner = floorPos + Vector2Int.up + Vector2Int.right;
                    if (!floorPositions.Contains(corner)) wallPositions.Add(corner);
                }
                if (!floorPositions.Contains(floorPos + Vector2Int.up) &&
                    !floorPositions.Contains(floorPos + Vector2Int.left))
                {
                    Vector2Int corner = floorPos + Vector2Int.up + Vector2Int.left;
                    if (!floorPositions.Contains(corner)) wallPositions.Add(corner);
                }
                if (!floorPositions.Contains(floorPos + Vector2Int.down) &&
                    !floorPositions.Contains(floorPos + Vector2Int.right))
                {
                    Vector2Int corner = floorPos + Vector2Int.down + Vector2Int.right;
                    if (!floorPositions.Contains(corner)) wallPositions.Add(corner);
                }
                if (!floorPositions.Contains(floorPos + Vector2Int.down) &&
                    !floorPositions.Contains(floorPos + Vector2Int.left))
                {
                    Vector2Int corner = floorPos + Vector2Int.down + Vector2Int.left;
                    if (!floorPositions.Contains(corner)) wallPositions.Add(corner);
                }
            }
            // Instantiate appropriate wall prefab for each wall position
            foreach (var pos in wallPositions)
            {
                if (floorPositions.Contains(pos)) continue; // skip if somehow marked as floor too
                bool wallUp = wallPositions.Contains(pos + Vector2Int.up);
                bool wallDown = wallPositions.Contains(pos + Vector2Int.down);
                bool wallLeft = wallPositions.Contains(pos + Vector2Int.left);
                bool wallRight = wallPositions.Contains(pos + Vector2Int.right);
                bool floorNE = floorPositions.Contains(pos + Vector2Int.up + Vector2Int.right);
                bool floorNW = floorPositions.Contains(pos + Vector2Int.up + Vector2Int.left);
                bool floorSE = floorPositions.Contains(pos + Vector2Int.down + Vector2Int.right);
                bool floorSW = floorPositions.Contains(pos + Vector2Int.down + Vector2Int.left);

                GameObject prefabToUse = null;
                Quaternion rotation = Quaternion.identity;
                // Determine wall type (corner vs straight vs end) based on neighbors
                if (wallUp && wallRight && !wallDown && !wallLeft)
                {
                    prefabToUse = (floorNE ? wallInnerCornerPrefab : wallOuterCornerPrefab);
                    rotation = Quaternion.Euler(0, 0, 0); // opens toward bottom-left
                    debugText = "opens toward bottom-left";
                }
                else if (wallUp && wallLeft && !wallDown && !wallRight)
                {
                    prefabToUse = (floorNW ? wallInnerCornerPrefab : wallOuterCornerPrefab);
                    rotation = Quaternion.Euler(0, 0, 90); // opens toward bottom-right
                    debugText = "opens toward bottom-right";
                }
                else if (wallDown && wallRight && !wallUp && !wallLeft)
                {
                    prefabToUse = (floorSE ? wallInnerCornerPrefab : wallOuterCornerPrefab);
                    rotation = Quaternion.Euler(0, 0, -90); // opens toward top-left
                    debugText = "opens toward top-left";
                }
                else if (wallDown && wallLeft && !wallUp && !wallRight)
                {
                    prefabToUse = (floorSW ? wallInnerCornerPrefab : wallOuterCornerPrefab);
                    rotation = Quaternion.Euler(0, 0, 180); // opens toward top-right
                    debugText = "opens toward top-right";
                }
                // Straight walls
                else if ((wallLeft && wallRight) && !wallUp && !wallDown)
                {
                    prefabToUse = wallHorizontalPrefab;
                    rotation = Quaternion.identity; // horizontal segment
                    debugText = "horizontal segment";
                }
                else if ((wallUp && wallDown) && !wallLeft && !wallRight)
                {
                    prefabToUse = wallVerticalPrefab;
                    // Flip vertical wall if needed (depending on adjacent floor for visual consistency)
                    rotation = floorPositions.Contains(pos + Vector2Int.right)
                               ? Quaternion.Euler(0, 0, 180)
                               : Quaternion.identity;
                }
                else
                {
                    // End-piece or single wall
                    if (wallLeft || wallRight)
                    {
                        prefabToUse = wallHorizontalPrefab;
                        rotation = Quaternion.identity;
                        debugText = "End-piece or single wall";
                    }
                    else if (wallUp || wallDown)
                    {
                        prefabToUse = wallVerticalPrefab;
                        rotation = Quaternion.identity;
                        debugText = "Wall up or down";
                    }
                    else
                    {
                        prefabToUse = wallHorizontalPrefab; // isolated wall (rare)
                        rotation = Quaternion.identity;
                        debugText = "isolated wall (rare)";
                    }
                }
                // Instantiate the chosen wall piece
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0f);
                GameObject wallPiece = Instantiate(prefabToUse, worldPos, rotation, GameManager.Instance.DungeonParent);
                wallPiece.name = $"[ROGUE] DebugObj ({debugText}, orinal name {wallPiece.name})";
            }
        }

        /// <summary>Get the Tile component at a given grid position (if exists).</summary>
        public Tile GetTile(Vector2Int pos)
        {
            tiles.TryGetValue(pos, out var tile);
            return tile;
        }

        /// <summary>Returns a set of all floor positions in the dungeon.</summary>
        public HashSet<Vector2Int> GetFloorPositions()
        {
            // Return a copy to prevent external modifications
            return new HashSet<Vector2Int>(floorPositions);
        }
    }
}
