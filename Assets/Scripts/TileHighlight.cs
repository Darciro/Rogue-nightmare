using UnityEngine;

public class TileHighlight : MonoBehaviour
{
    private Vector2Int gridPos;
    private bool isAttack;

    public void Init(Vector2Int pos, bool attack = false)
    {
        gridPos = pos;
        isAttack = attack;

        // change color based on type
        GetComponent<SpriteRenderer>().color = attack ? Color.red : Color.cyan;
    }

    private void OnMouseDown()
    {
        if (isAttack)
            GameManager.Instance.TryAttackTarget(gridPos);
        else
            GameManager.Instance.TryMoveSelectedCharacter(gridPos);
    }

}
