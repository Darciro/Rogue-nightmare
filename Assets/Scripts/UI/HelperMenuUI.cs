namespace RogueNightmare.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using RogueNightmare.Core;
    
    public class HelperMenuUI : MonoBehaviour
    {
        public static HelperMenuUI Instance { get; private set; }
        
        [SerializeField] private Button turnEndButton;
        
        void Awake()
        {
            Instance = this;
            // Could add: turnEndButton.onClick.AddListener(OnTurnEndPressed);
        }
        
        public void OnTurnEndPressed()
        {
            // End the current turn via GameManager (which updates turn UI as well)
            GameManager.Instance.NextTurn();
        }
    }
}