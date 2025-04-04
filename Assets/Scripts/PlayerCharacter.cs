using UnityEngine;

public class PlayerCharacter : CharacterBase
{
    protected override void OnTurnStart()
    {
        Debug.Log($"{characterName}'s turn! Awaiting player input...");
        // GameManager.Instance.ShowMoveRange(gridPosition, 3); // 3 = move range
    }

    private void OnMouseDown()
    {
        if (!isMyTurn) return;

        GameManager.Instance.ShowMoveRange(this, 3); // Or make move range a variable
    }


    void Update()
    {
        if (!isMyTurn) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    void TryMove(Vector2Int direction)
    {
        Vector2Int targetPos = gridPosition + direction;

        if (!GameManager.Instance.IsWalkable(targetPos))
            return;

        MoveTo(targetPos);

        EndTurn(); // Movement ends the turn
    }
}
