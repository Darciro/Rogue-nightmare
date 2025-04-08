using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CharacterBase : MonoBehaviour
{

    [Header("Attributes")]
    public int Strength = 1;        // Melee damage
    public int Dexterity = 1;       // Ranged damage
    public int Constitution = 1;    // Health points
    public int Intelligence = 1;    // Magic
    public int Percepction = 1;     // Senses

    [Header("Stats")]
    public int maxHP = 10;
    public int currentHP = 10;

    public int maxActionPoints = 3;
    public int currentActionPoints = 3;

    public int foodPoints = 100;
    public int waterPoints = 100;

    public int attackDamage = 2;
    public int attackRange = 1;


    [Header("Turn System")]
    public string characterName;
    public bool isMyTurn;
    public Vector2Int gridPosition;
    public float moveSpeed = 5f;

    protected virtual void Start()
    {
        // gridPosition = GameManager.Instance.WorldToGrid(transform.position);
        currentHP = maxHP;
    }

    public void StartTurn()
    {
        isMyTurn = true;
        currentActionPoints = maxActionPoints;
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
        var path = GameManager.Instance.FindPath(gridPosition, target, currentActionPoints);
        if (path == null) return;

        StartCoroutine(MoveAlongPath(path));
    }

    public IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        foreach (Vector2Int step in path)
        {
            if (currentActionPoints <= 0) break;

            // Clear old tile
            GameManager.Instance.GetTile(gridPosition).occupant = null;

            Vector3 worldPos = GameManager.Instance.GridToWorld(step);

            while ((transform.position - worldPos).sqrMagnitude > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, worldPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            gridPosition = step;
            GameManager.Instance.GetTile(gridPosition).occupant = this.gameObject;
            currentActionPoints--;

            GameManager.Instance.UpdatePlayerUI();
        }

        GameManager.Instance.ClearHighlights();
    }

    public IEnumerator SmoothMove(Vector2Int target)
    {
        Vector3 targetWorld = new Vector3(target.x, target.y, 0f);

        while ((transform.position - targetWorld).sqrMagnitude > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetWorld;
        gridPosition = target;

        currentActionPoints--;

        if (isMyTurn && this is PlayerCharacter && currentActionPoints > 0)
        {
            GameManager.Instance.ShowMoveRange(this, currentActionPoints);
        }
    }


    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{characterName} takes {damage} damage! ({currentHP}/{maxHP})");

        ShowDamagePopup(damage);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{characterName} has died!");

        // Remove from turn order
        GameManager.Instance.RemoveCharacter(this);

        // Remove from tile
        // GameManager.Instance.GetTile(gridPosition).occupant = null;

        // Destroy game object
        Destroy(gameObject);

        if (this is PlayerCharacter)
        {
            GameManager.Instance.gameOverScreen.SetActive(true);
        }
    }

    void ShowDamagePopup(int amount)
    {
        if (GameManager.Instance.damagePopupPrefab == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        GameObject popupGO = Instantiate(GameManager.Instance.damagePopupPrefab, screenPos, Quaternion.identity, GameObject.Find("Canvas").transform);

        DamagePopup popup = popupGO.GetComponent<DamagePopup>();
        popup.Setup(amount);
    }


    // To be implemented by Player or Enemy
    protected abstract void OnTurnStart();
}