using UnityEngine;
using UnityEngine.UI;

public class HelperMenuUI : MonoBehaviour
{
    public static HelperMenuUI Instance;

    [Header("UI Buttons")]
    public Button turnEndButton;

    void Awake()
    {
        Instance = this;
    }

    public void OnTurnEndPressed()
    {
        GameManager.Instance.turnManager.NextTurn();
    }

}
