namespace RogueNightmare.Managers
{
    using System.Collections.Generic;
    using UnityEngine;
    using RogueNightmare.Characters;
    
    public class TurnManager : MonoBehaviour
    {
        public List<CharacterManager> TurnOrder { get; private set; } = new List<CharacterManager>();
        private int currentTurnIndex = 0;

        // Convenience property to get the character whose turn it is.
        public CharacterManager CurrentCharacter => 
            (TurnOrder.Count > 0 ? TurnOrder[currentTurnIndex] : null);

        /// <summary>Set up the turn order list at game start.</summary>
        public void InitializeTurnOrder(List<CharacterManager> characters)
        {
            TurnOrder = new List<CharacterManager>(characters);
            currentTurnIndex = 0;
        }

        /// <summary>Begin the first turn in the list.</summary>
        public void StartFirstTurn()
        {
            if (TurnOrder.Count > 0)
            {
                TurnOrder[0].StartTurn();
            }
        }

        /// <summary>Ends the current turn and starts the next one.</summary>
        public void NextTurn()
        {
            if (TurnOrder == null || TurnOrder.Count == 0)
            {
                Debug.LogWarning("[TurnManager] No turn order defined!");
                return;
            }
            // Advance index cyclically
            currentTurnIndex = (currentTurnIndex + 1) % TurnOrder.Count;
            CharacterManager next = TurnOrder[currentTurnIndex];
            if (next == null)
            {
                Debug.LogError("[TurnManager] Next turn's character is null.");
                return;
            }
            // Start next character's turn
            next.StartTurn();
            Debug.Log($"[TurnManager] Now it's {next.characterName}'s turn.");
        }

        /// <summary>Removes a character from turn order (e.g. on death).</summary>
        public void RemoveFromTurnOrder(CharacterManager character)
        {
            TurnOrder.Remove(character);
            // Adjust index to avoid out-of-range if last element was removed
            if (currentTurnIndex >= TurnOrder.Count)
                currentTurnIndex = 0;
        }

        /// <summary>Peek the upcoming turn order (next few turns).</summary>
        public List<CharacterManager> GetUpcomingTurns(int count = 3)
        {
            var upcoming = new List<CharacterManager>();
            if (TurnOrder.Count == 0) return upcoming;
            for (int i = 0; i < Mathf.Min(count, TurnOrder.Count); i++)
            {
                int idx = (currentTurnIndex + i) % TurnOrder.Count;
                upcoming.Add(TurnOrder[idx]);
            }
            return upcoming;
        }
    }
}
