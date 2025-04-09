namespace RogueNightmare.UI
{
    using UnityEngine;
    using UnityEngine.UIElements;
    using RogueNightmare.Characters;
    
    public class EnemyTooltipUI : MonoBehaviour
    {
        public static EnemyTooltipUI Instance { get; private set; }
        
        private VisualElement root;
        private VisualElement tooltip;
        private Label nameLabel, hpLabel, strLabel, dexLabel, conLabel, intLabel, perLabel;
        
        void Awake()
        {
            Instance = this;
            // Initialize UIDocument and VisualElement references
            var uiDoc = GetComponent<UIDocument>();
            root = uiDoc.rootVisualElement;
            tooltip = root.Q<VisualElement>("enemy-tooltip");
            nameLabel = root.Q<Label>("enemy-name");
            hpLabel   = root.Q<Label>("enemy-hp");
            strLabel  = root.Q<Label>("enemy-str");
            dexLabel  = root.Q<Label>("enemy-dex");
            conLabel  = root.Q<Label>("enemy-con");
            intLabel  = root.Q<Label>("enemy-int");
            perLabel  = root.Q<Label>("enemy-per");
            Hide(); // hide tooltip initially
        }
        
        public void Show(CharacterManager enemy)
        {
            if (enemy == null) return;
            nameLabel.text = enemy.characterName;
            hpLabel.text   = $"HP: {enemy.currentHP}/{enemy.maxHP}";
            strLabel.text  = $"STR: {enemy.Strength}";
            dexLabel.text  = $"DEX: {enemy.Dexterity}";
            conLabel.text  = $"CON: {enemy.Constitution}";
            intLabel.text  = $"INT: {enemy.Intelligence}";
            perLabel.text  = $"PER: {enemy.Perception}";
            // Position tooltip near the enemy's screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
            tooltip.style.left = screenPos.x;
            tooltip.style.top  = Screen.height - screenPos.y; // convert to UI Toolkit coordinate space
            tooltip.style.display = DisplayStyle.Flex;
        }
        
        public void Hide()
        {
            if (tooltip != null)
            {
                tooltip.style.display = DisplayStyle.None;
            }
        }
    }
}
