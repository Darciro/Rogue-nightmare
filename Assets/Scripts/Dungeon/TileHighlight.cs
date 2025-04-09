using RogueNightmare.Characters;
using RogueNightmare.Core;
using RogueNightmare.Managers;

namespace RogueNightmare.Dungeon
{
    using UnityEngine;

    public class TileHighlight : MonoBehaviour
    {
        public Vector2Int gridPosition;
        public bool isAttackTile;

        public Sprite moveSprite;
        public Sprite attackSprite;

        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(Vector2Int pos, bool isAttack = false)
        {
            gridPosition = pos;
            isAttackTile = isAttack;

            spriteRenderer.sprite = isAttack ? attackSprite : moveSprite;
            spriteRenderer.color = isAttack ? Color.red : Color.cyan;
        }

        void OnMouseDown()
        {
            if (isAttackTile)
                GameManager.Instance.TryAttackTarget(gridPosition);
            else
                GameManager.Instance.TryMoveSelectedCharacter(gridPosition);
        }

        void OnMouseEnter()
        {
            if (!isAttackTile)
            {
                // Shows the path preview when hovering over a move tile.
                CharacterManager selected = GameManager.Instance.SelectedCharacter;
                if (selected != null)
                {
                    var path = GameManager.Instance.FindPath(selected.gridPosition, gridPosition, selected.currentActionPoints);
                    if (path != null)
                        PathPreviewManager.Instance.ShowPath(path);
                }
            }
            else
            {
                // Handle hovering over attack tile (optional extra highlighting)
                HighlightEnemyUnderAttack();
            }
        }

        void OnMouseExit()
        {
            if (!isAttackTile)
            {
                PathPreviewManager.Instance.Clear();
            }
            else
            {
                ClearEnemyHighlight();
            }
        }

        void HighlightEnemyUnderAttack()
        {
            // Optional: highlight the enemy or provide visual feedback
            Tile tile = GameManager.Instance.GetTile(gridPosition);
            if (tile != null && tile.occupant != null)
            {
                var enemy = tile.occupant.GetComponent<CharacterManager>();
                if (enemy != null)
                    enemy.Highlight(true);
            }
        }

        void ClearEnemyHighlight()
        {
            // Optional: remove the enemy highlight
            Tile tile = GameManager.Instance.GetTile(gridPosition);
            if (tile != null && tile.occupant != null)
            {
                var enemy = tile.occupant.GetComponent<CharacterManager>();
                if (enemy != null)
                    enemy.Highlight(false);
            }
        }
    }
}