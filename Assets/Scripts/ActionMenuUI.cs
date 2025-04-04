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


        /* if (this is PlayerCharacter)
        {
            GameManager.Instance.ShowMoveRange(this, currentActionPoints);
        } */

        Hide();
    }

    public void OnCancelPressed()
    {
        Hide();
    }

    // TODO: Hook up attack and item later
}
