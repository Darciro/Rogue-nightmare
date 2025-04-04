using System.Collections;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{

    [Header("Stats")]
    public int maxHP = 10;
    public int currentHP;
    public int attackDamage = 2;


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
        if (currentActionPoints <= 0)
        {
            Debug.Log($"{characterName} has no action points left, can not move!");
            return;
        }

        StartCoroutine(SmoothMove(target));
    }

    public IEnumerator SmoothMove(Vector2Int target)
    {
        /* if (currentActionPoints <= 0)
        {
            Debug.Log($"{characterName} has no action points left!");
            yield break;
        }*/

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
        GameManager.Instance.GetTile(gridPosition).occupant = null;

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