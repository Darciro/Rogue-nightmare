using RogueNightmare.Core;

namespace RogueNightmare.Characters
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using RogueNightmare.Managers;
    using RogueNightmare.Dungeon;
    using RogueNightmare.UI;
    
    public abstract class CharacterManager : MonoBehaviour
    {
        [Header("Attributes")]
        [SerializeField] private int strength = 1;      // Melee damage base
        [SerializeField] private int dexterity = 1;     // Ranged damage base
        [SerializeField] private int constitution = 1;  // Health points base
        [SerializeField] private int intelligence = 1;  // Magic power base
        [SerializeField] private int perception = 1;    // Senses base

        [Header("Stats")]
        public int maxHP = 10;
        public int maxActionPoints = 3;
        public int attackDamage = 2;
        public int attackRange = 1;
        
        // Current dynamic values (not serialized)
        public int currentHP { get; private set; }
        public int currentActionPoints { get; private set; } = 3;
        public int foodPoints = 100;
        public int waterPoints = 100;
        
        [Header("Turn & Movement")]
        public string characterName;
        public bool isMyTurn { get; private set; }
        public Vector2Int gridPosition;
        [SerializeField] private float moveSpeed = 5f;
        
        protected virtual void Start()
        {
            // Initialize runtime stats
            currentHP = maxHP;
            currentActionPoints = maxActionPoints;
        }

        /// <summary>Called by TurnManager to mark the beginning of this character's turn.</summary>
        public void StartTurn()
        {
            isMyTurn = true;
            currentActionPoints = maxActionPoints;
            OnTurnStart();  // trigger character-specific turn start behavior
        }

        /// <summary>Ends this character's turn and notifies the game to proceed to next turn.</summary>
        public void EndTurn()
        {
            isMyTurn = false;
            GameManager.Instance.NextTurn();  // delegate turn advancement to GameManager/TurnManager
        }
        
        public void UseActionPoints(int amount = 1)
        {
            currentActionPoints = Mathf.Max(currentActionPoints - amount, 0);
        }


        /// <summary>Tries to move the character along a path toward a target grid position.</summary>
        public void MoveTo(Vector2Int targetGridPos)
        {
            // Find a path within the character's remaining action points range
            List<Vector2Int> path = GameManager.Instance.FindPath(gridPosition, targetGridPos, currentActionPoints);
            Debug.Log($"[MoveTo] {characterName}: AP={currentActionPoints}, PathLength={path?.Count}");
            if (path == null || path.Count == 0) return;  // no valid path
            StartCoroutine(MoveAlongPath(path));
        }

        /// <summary>Coroutine to smoothly move the character along a path tile-by-tile.</summary>
        public IEnumerator MoveAlongPath(List<Vector2Int> path)
        {
            foreach (Vector2Int step in path)
            {
                if (currentActionPoints <= 0) break;
                // Free the current tile
                Tile currentTile = GameManager.Instance.GetTile(gridPosition);
                if (currentTile != null) currentTile.occupant = null;
                // Move towards the next step
                Vector3 targetPos = GameManager.Instance.GridToWorld(step);
                while ((transform.position - targetPos).sqrMagnitude > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }
                // Snap to position and update grid
                transform.position = targetPos;
                gridPosition = step;
                // Occupy the new tile
                Tile nextTile = GameManager.Instance.GetTile(gridPosition);
                if (nextTile != null) nextTile.occupant = this.gameObject;
                // Spend action point
                currentActionPoints--;
                GameManager.Instance.UpdatePlayerUI();  // refresh UI if player
                if (currentActionPoints <= 0)
                {
                    // If out of AP, end turn immediately
                    GameManager.Instance.NextTurn();
                    yield break;
                }
            }
            // After moving, reactivate action menu and clear move previews/highlights
            ActionMenuUI.Instance.ShowActionButtons();
            GameManager.Instance.ClearHighlights();
            PathPreviewManager.Instance.Clear();
        }

        /// <summary>Direct, immediate movement to a target tile (used by AI or specific cases).</summary>
        public IEnumerator SmoothMove(Vector2Int target)
        {
            Vector3 targetWorld = new Vector3(target.x, target.y, 0f);
            // Smoothly translate to the target position
            while ((transform.position - targetWorld).sqrMagnitude > 0.001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetWorld;
            gridPosition = target;
            currentActionPoints--;
            // If this character is the player and still has AP, show updated move range
            if (isMyTurn && this is PlayerCharacter && currentActionPoints > 0)
            {
                GameManager.Instance.ShowMoveRange(this, currentActionPoints);
            }
        }

        /// <summary>Apply damage to this character and handle death.</summary>
        public void TakeDamage(int damage)
        {
            currentHP -= damage;
            Debug.Log($"{characterName} takes {damage} damage! ({currentHP}/{maxHP})");
            ShowDamagePopup(damage);
            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>Handles character death: removal from turn order and destroying the GameObject.</summary>
        protected virtual void Die()
        {
            Debug.Log($"{characterName} has died.");
            GameManager.Instance.RemoveCharacter(this);
            Destroy(gameObject);
            if (this is PlayerCharacter)
            {
                GameManager.Instance.ShowGameOver();  // trigger game over UI if player died
            }
        }

        // Shows a floating damage number using the DamagePopup UI prefab
        private void ShowDamagePopup(int amount)
        {
            var popupPrefab = GameManager.Instance.damagePopupPrefab;
            if (popupPrefab == null) return;
            // Spawn the popup at character's position in screen space, under the Canvas
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
            // GameObject popupGO = Instantiate(popupPrefab, screenPos, Quaternion.identity, UIManager.CanvasTransform);
            GameObject popupGO = Instantiate(GameManager.Instance.damagePopupPrefab, screenPos, Quaternion.identity, GameObject.Find("Canvas").transform);

            // UIManager.CanvasTransform is a hypothetical reference to the main Canvas transform
            DamagePopup popup = popupGO.GetComponent<DamagePopup>();
            popup.Setup(amount);
        }
        
        public void Highlight(bool active)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = active ? Color.red : Color.white;
            }
        }


        // Each character type implements what happens at the start of their turn.
        protected abstract void OnTurnStart();
        
        // Public getters for core attributes (if needed outside)
        public int Strength => strength;
        public int Dexterity => dexterity;
        public int Constitution => constitution;
        public int Intelligence => intelligence;
        public int Perception => perception;
    }
}
