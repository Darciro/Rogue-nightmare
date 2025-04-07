using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<CharacterBase> TurnOrder { get; private set; } = new List<CharacterBase>();
    private int currentTurnIndex = 0;

    public CharacterBase CurrentCharacter => TurnOrder.Count > 0 ? TurnOrder[currentTurnIndex] : null;

    /// <summary>
    /// Initializes the turn order with the given list of characters.
    /// </summary>
    public void InitializeTurnOrder(List<CharacterBase> characters)
    {
        TurnOrder = new List<CharacterBase>(characters);
        currentTurnIndex = 0;
    }

    /// <summary>
    /// Starts the first character's turn.
    /// </summary>
    public void StartFirstTurn()
    {
        if (TurnOrder.Count > 0)
        {
            TurnOrder[0].StartTurn();
        }
    }

    /// <summary>
    /// Moves to the next character and starts their turn.
    /// </summary>
    public void NextTurn()
    {
        if (TurnOrder.Count == 0) return;

        currentTurnIndex = (currentTurnIndex + 1) % TurnOrder.Count;
        TurnOrder[currentTurnIndex].StartTurn();
    }

    /// <summary>
    /// Removes a character from the turn order.
    /// </summary>
    public void RemoveFromTurnOrder(CharacterBase character)
    {
        TurnOrder.Remove(character);

        // Prevent index overflow
        if (currentTurnIndex >= TurnOrder.Count)
        {
            currentTurnIndex = 0;
        }
    }

    /// <summary>
    /// Returns the next X characters in the turn order.
    /// </summary>
    public List<CharacterBase> GetUpcomingTurns(int count = 3)
    {
        List<CharacterBase> upcoming = new List<CharacterBase>();

        for (int i = 0; i < Mathf.Min(count, TurnOrder.Count); i++)
        {
            int index = (currentTurnIndex + i) % TurnOrder.Count;
            upcoming.Add(TurnOrder[index]);
        }

        return upcoming;
    }
}
