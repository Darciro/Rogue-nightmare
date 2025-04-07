using UnityEngine;
using UnityEngine.UI;

public class ActionMenuUI : MonoBehaviour
{
    public static ActionMenuUI Instance;

    [Header("UI Buttons")]
    public Button moveButton;
    public Button attackButton;
    public Button itemButton;
    public Button cancelButton;

    private CharacterBase selectedCharacter;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(CharacterBase character)
    {
        selectedCharacter = character;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        selectedCharacter = null;
    }

    public void OnMovePressed()
    {
        if (selectedCharacter is PlayerCharacter player)
        {
            GameManager.Instance.ShowMoveRange(player, player.currentActionPoints);
        }

        Hide();
    }

    public void OnAttackPressed()
    {
        /* if (selectedCharacter is PlayerCharacter player)
        {
            GameManager.Instance.ShowAttackRange(player, player.attackRange);
        }

        Hide(); */
    }

    public void OnCancelPressed()
    {
        Hide();
    }

}
