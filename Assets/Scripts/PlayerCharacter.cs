using UnityEngine;

public class PlayerCharacter : CharacterBase
{
    protected override void OnTurnStart()
    {
        if (!isMyTurn) return;

        // Only open the action menu — no move, no highlight, no AP use
        ActionMenuUI.Instance.Show(this);
    }


    void Update()
    {
        if (!isMyTurn) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }
}
