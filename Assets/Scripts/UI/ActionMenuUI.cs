using RogueNightmare.Managers;

namespace RogueNightmare.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using RogueNightmare.Characters;
    using RogueNightmare.Core;
    
    public class ActionMenuUI : MonoBehaviour
    {
        public static ActionMenuUI Instance { get; private set; }
        
        [Header("Buttons")]
        [SerializeField] private Button moveButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject playerStatsPanel;
        
        void Awake()
        {
            Instance = this;
            // Ensure menu starts hidden
            gameObject.SetActive(false);
            if (playerStatsPanel != null) 
                playerStatsPanel.SetActive(false);
        }
        
        public void Show(CharacterManager character)
        {
            gameObject.SetActive(true);
            if (playerStatsPanel) playerStatsPanel.SetActive(true);
            ShowActionButtons();
        }
        
        public void HideMenu()
        {
            gameObject.SetActive(false);
        }
        
        // UI Button Handlers:
        public void OnMovePressed()
        {
            if (GameManager.Instance.turnManager.CurrentCharacter is PlayerCharacter player)
            {
                GameManager.Instance.ShowMoveRange(player, player.currentActionPoints);
            }
            ShowCancelOnly();
        }
        
        public void OnAttackPressed()
        {
            if (GameManager.Instance.turnManager.CurrentCharacter is PlayerCharacter player)
            {
                GameManager.Instance.ShowAttackRange(player, player.attackRange);
            }
            ShowCancelOnly();
        }
        
        public void OnCancelPressed()
        {
            GameManager.Instance.ClearHighlights();
            PathPreviewManager.Instance.Clear();
            ShowActionButtons();
        }
        
        // Toggle which buttons are visible:
        public void ShowActionButtons()
        {
            moveButton.gameObject.SetActive(true);
            attackButton.gameObject.SetActive(true);
            itemButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);
        }
        
        public void ShowCancelOnly()
        {
            moveButton.gameObject.SetActive(false);
            attackButton.gameObject.SetActive(false);
            itemButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(true);
        }
    }
}
