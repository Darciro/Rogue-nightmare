using UnityEngine;
using System.Collections;

public class EnemyCharacter : CharacterBase
{
    protected override void OnTurnStart()
    {
        Debug.Log($"{characterName}'s turn (AI)");
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        while (currentActionPoints > 0)
        {
            PlayerCharacter target = FindNearestPlayer();
            if (target == null)
            {
                Debug.LogWarning($"{characterName} found no target.");
                break;
            }

            if (IsAdjacent(target.gridPosition))
            {
                Debug.Log($"{characterName} attacks {target.characterName}!");
                target.TakeDamage(attackDamage);
                yield return new WaitForSeconds(0.5f); // simulate attack delay
                break; // For now: attack ends turn
            }

            Vector2Int direction = GetStepToward(target.gridPosition);
            Vector2Int newPos = gridPosition + direction;

            if (GameManager.Instance.IsWalkable(newPos))
            {
                Debug.Log($"{characterName} moves to {newPos}");
                yield return StartCoroutine(SmoothMove(newPos));
            }
            else
            {
                Debug.Log($"{characterName} blocked at {newPos}, ending turn.");
                break;
            }

            // Wait a short time before next move (optional for pacing)
            yield return new WaitForSeconds(0.1f);
        }

        EndTurn(); // End after all actions or early stop
    }

    bool IsAdjacent(Vector2Int targetPos)
    {
        int dx = Mathf.Abs(gridPosition.x - targetPos.x);
        int dy = Mathf.Abs(gridPosition.y - targetPos.y);
        return (dx + dy) == 1;
    }

    Vector2Int GetStepToward(Vector2Int target)
    {
        int dx = target.x - gridPosition.x;
        int dy = target.y - gridPosition.y;

        if (Mathf.Abs(dx) > Mathf.Abs(dy))
            return new Vector2Int((int)Mathf.Sign(dx), 0);
        else
            return new Vector2Int(0, (int)Mathf.Sign(dy));
    }

    PlayerCharacter FindNearestPlayer()
    {
        PlayerCharacter closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var character in GameManager.Instance.turnManager.TurnOrder)
        {
            if (character is PlayerCharacter player)
            {
                float dist = Vector2Int.Distance(gridPosition, player.gridPosition);
                if (dist < closestDist)
                {
                    closest = player;
                    closestDist = dist;
                }
            }
        }

        return closest;
    }


}
