using UnityEngine;
using UnityEngine.UIElements;

public class EnemyTooltipUI : MonoBehaviour
{
    private VisualElement root;
    private VisualElement tooltip;

    private Label nameLabel;
    private Label hpLabel;
    private Label strLabel;
    private Label dexLabel;
    private Label conLabel;
    private Label intLabel;
    private Label perLabel;

    public static EnemyTooltipUI Instance;

    void Awake()
    {
        Instance = this;

        var uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;

        tooltip = root.Q<VisualElement>("enemy-tooltip");

        nameLabel = root.Q<Label>("enemy-name");
        hpLabel = root.Q<Label>("enemy-hp");
        strLabel = root.Q<Label>("enemy-str");
        dexLabel = root.Q<Label>("enemy-dex");
        conLabel = root.Q<Label>("enemy-con");
        intLabel = root.Q<Label>("enemy-int");
        perLabel = root.Q<Label>("enemy-per");

        Hide();
    }

    public void Show(CharacterBase enemy)
    {
        nameLabel.text = enemy.characterName;
        hpLabel.text = $"HP: {enemy.currentHP}/{enemy.maxHP}";
        strLabel.text = $"STR: {enemy.Strength}";
        dexLabel.text = $"DEX: {enemy.Dexterity}";
        conLabel.text = $"CON: {enemy.Constitution}";
        intLabel.text = $"INT: {enemy.Intelligence}";
        perLabel.text = $"PER: {enemy.Percepction}";

        // World → Screen → UI Position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
        tooltip.style.left = screenPos.x;
        tooltip.style.top = Screen.height - screenPos.y; // invert Y for UI Toolkit
        tooltip.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        tooltip.style.display = DisplayStyle.None;
    }
}
