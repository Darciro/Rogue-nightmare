using RogueNightmare.UI;

namespace RogueNightmare.Characters
{
    using UnityEngine;
    
    public class PlayerCharacter : CharacterManager
    {
        protected override void OnTurnStart()
        {
            if (!isMyTurn) return;
            // When the player's turn starts, show the action menu UI
            ActionMenuUI.Instance.Show(this);
        }
        
        void Update()
        {
            if (!isMyTurn) return;
            // Allow ending turn with Space key as a convenience
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndTurn();
            }
        }
        
        // Expose max values for UI usage
        public int MaxHP => base.currentHP;             // currentHP is initialized to max in Start
        public int MaxActionPoints => base.maxActionPoints;
    }
}