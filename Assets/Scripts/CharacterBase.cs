using System.Collections;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [Header("Turn System")]
    public int maxActionPoints = 3;
    public int currentActionPoints;
    public string characterName;
    public bool isMyTurn;
    public Vector2Int gridPosition;
    public float moveSpeed = 5f;

    protected virtual void Start()
    {
        gridPosition = GameManager.Instance.WorldToGrid(transform.position);
    }

    public void StartTurn()
    {
        isMyTurn = true;
        currentActionPoints = maxActionPoints;

        if (this is PlayerCharacter)
        {
            GameManager.Instance.ShowMoveRange(this, currentActionPoints);
        }

        OnTurnStart();
    }

    public void EndTurn()
    {
        isMyTurn = false;

        // Display turn info
        // GameManager.Instance?.SetTurn($"{characterName}'s Turn");

        GameManager.Instance.NextTurn();
    }

    public void MoveTo(Vector2Int target)
    {
        if (currentActionPoints <= 0)
        {
            Debug.Log($"{characterName} has no action points left, can not move!");
            return;
        }

        StartCoroutine(SmoothMove(target));
    }

    public IEnumerator SmoothMove(Vector2Int target)
    {
        if (currentActionPoints <= 0)
        {
            Debug.Log($"{characterName} has no action points left!");
            yield break;
        }

        // 🧼 CLEAR current tile occupant
        GameManager.Instance.GetTile(gridPosition).occupant = null;

        Vector3 targetWorld = GameManager.Instance.GridToWorld(target);

        while ((transform.position - targetWorld).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetWorld;
        gridPosition = target;

        GameManager.Instance.GetTile(gridPosition).occupant = this.gameObject;

        currentActionPoints--;

        if (isMyTurn && this is PlayerCharacter)
        {
            GameManager.Instance.ShowMoveRange(this, currentActionPoints);
        }

    }



    // To be implemented by Player or Enemy
    protected abstract void OnTurnStart();
}