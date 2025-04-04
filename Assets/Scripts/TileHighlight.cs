using UnityEngine;

public class TileHighlight : MonoBehaviour
{
    public Vector2Int gridPos;

    public void Init(Vector2Int pos)
    {
        gridPos = pos;
    }

    private void OnMouseDown()
    {
        GameManager.Instance.TryMoveSelectedCharacter(gridPos);
    }
}
