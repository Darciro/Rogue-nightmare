using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool walkable = true;

    public GameObject occupant; // Can be player, enemy, item, wall, etc.

    public void Init(Vector2Int pos, bool isWalkable = true)
    {
        gridPosition = pos;
        walkable = isWalkable;
        name = $"Tile_{pos.x}_{pos.y}";
    }

    public bool IsOccupied()
    {
        return occupant != null;
    }
}
