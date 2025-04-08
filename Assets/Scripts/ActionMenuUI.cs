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
    public GameObject playerStatsUI;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
        playerStatsUI.SetActive(false);
    }

    public void Show(CharacterBase character)
    {
        gameObject.SetActive(true);
        playerStatsUI.SetActive(true);
        ShowActionButtons();
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
    }

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
        ShowActionButtons();
    }

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
