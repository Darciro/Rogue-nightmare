using System;
using RogueNightmare.Core;
using RogueNightmare.UI;

namespace RogueNightmare.Characters
{
    using UnityEngine;
    using System.Collections;
    
    public class EnemyCharacter : CharacterManager
    {
        // Show tooltip on mouse hover (requires collider on enemy object)
        private void OnMouseEnter()
        {
            EnemyTooltipUI.Instance.Show(this);
        }
        private void OnMouseExit()
        {
            EnemyTooltipUI.Instance.Hide();
        }
        
        protected override void OnTurnStart()
        {
            Debug.Log($"{characterName}'s turn (AI)");
            // Start enemy AI routine for its turn
            StartCoroutine(ExecuteTurn());
        }
        
        private IEnumerator ExecuteTurn()
        {
            // Basic AI: move towards nearest player and attack if adjacent
            while (currentActionPoints > 0)
            {
                PlayerCharacter targetPlayer = FindNearestPlayer();
                if (targetPlayer == null) break;  // no target
                if (IsAdjacent(targetPlayer.gridPosition))
                {
                    // If adjacent to player, attack
                    targetPlayer.TakeDamage(attackDamage);
                    yield return new WaitForSeconds(0.5f);  // small delay to visualize attack
                    break;  // end AI actions after attack
                }
                // Not adjacent: move one step toward the player
                Vector2Int stepDir = GetStepToward(targetPlayer.gridPosition);
                Vector2Int newPos = gridPosition + stepDir;
                if (GameManager.Instance.IsWalkable(newPos))
                {
                    // Use SmoothMove to move and decrement AP
                    yield return StartCoroutine(SmoothMove(newPos));
                }
                else
                {
                    // If blocked, end turn (could also try alternative paths in more advanced AI)
                    break;
                }
                // Short delay between moves (for visual pacing)
                yield return new WaitForSeconds(0.1f);
            }
            // End turn after actions
            EndTurn();
        }
        
        // Helper: check if a position is immediately adjacent (Manhattan distance 1)
        private bool IsAdjacent(Vector2Int targetPos)
        {
            int dx = Mathf.Abs(gridPosition.x - targetPos.x);
            int dy = Mathf.Abs(gridPosition.y - targetPos.y);
            return (dx + dy) == 1;
        }
        
        // Helper: get a unit step direction toward the target position
        private Vector2Int GetStepToward(Vector2Int targetPos)
        {
            int dx = targetPos.x - gridPosition.x;
            int dy = targetPos.y - gridPosition.y;
            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                return new Vector2Int(Mathf.Clamp(dx, -1, 1), 0);
            else
                return new Vector2Int(0, Mathf.Clamp(dy, -1, 1));
        }
        
        // Finds the nearest PlayerCharacter in the turn order (could be multiple players in theory)
        private PlayerCharacter FindNearestPlayer()
        {
            PlayerCharacter closest = null;
            float closestDist = Mathf.Infinity;
            foreach (CharacterManager character in GameManager.Instance.turnManager.TurnOrder)
            {
                if (character is PlayerCharacter player)
                {
                    float dist = Vector2Int.Distance(gridPosition, player.gridPosition);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = player;
                    }
                }
            }
            return closest;
        }

    }
}
