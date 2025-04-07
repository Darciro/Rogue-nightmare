using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float tileSize = 1f;

    [Header("UI")]
    public TextMeshProUGUI turnIndicator;
    public TextMeshProUGUI turnsOrder;
    public TextMeshProUGUI actionPointIndicator;
    public GameObject gameOverScreen;
    public GameObject damagePopupPrefab;

    [Header("Dungeon Settings")]
    public Transform dungeonWrapper;
    public DungeonGenerator dungeonGenerator;
    public GameObject moveHighlightPrefab;

    [Header("Entities")]
    public Transform entityParent;
    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;
    public GameObject[] pickupPrefabs;
    public GameObject[] wallPrefabs;

    [Header("Turns Management")]
    public TurnManager turnManager;

    private List<GameObject> activeHighlights = new List<GameObject>();
    private CharacterBase selectedCharacter;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Draws our dungeon
        dungeonGenerator.GenerateDungeon();

        // Invoke the chars
        SpawnEntities();

        // Initialize turns order
        CharacterBase[] characters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
        turnManager.InitializeTurnOrder(new List<CharacterBase>(characters));
        turnManager.StartFirstTurn();
    }

    void SpawnEntities()
    {
        List<Vector2Int> floorTiles = new List<Vector2Int>(dungeonGenerator.GetFloorPositions());

        // Spawn player
        Vector2Int playerPos = GetAndRemoveRandomTile(floorTiles);
        GameObject player = Instantiate(playerPrefab, GridToWorld(playerPos), Quaternion.identity, entityParent);
        player.GetComponent<CharacterBase>().gridPosition = playerPos;

        // Place camera above the player
        Camera.main.GetComponent<CameraFollow>().SetTarget(player.transform);

        // Spawn enemies
        int enemyCount = 3;
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int pos = GetAndRemoveRandomTile(floorTiles);
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, GridToWorld(pos), Quaternion.identity, entityParent);
            enemy.GetComponent<CharacterBase>().gridPosition = pos;
        }
    }

    Vector3 GridToWorld(Vector2Int pos)
    {
        return new Vector3(pos.x, pos.y, 0f);
    }

    Vector2Int GetAndRemoveRandomTile(List<Vector2Int> tiles)
    {
        int index = Random.Range(0, tiles.Count);
        Vector2Int tile = tiles[index];
        tiles.RemoveAt(index);
        return tile;
    }

    public void NextTurn()
    {
        turnManager.NextTurn();
        UpdateTurnUI();
        UpdateTurnQueueUI();
    }

    void UpdateTurnUI()
    {
        if (turnIndicator != null && turnManager.CurrentCharacter != null)
        {
            turnIndicator.text = $"{turnManager.CurrentCharacter.characterName}'s Turn";
        }

        if (actionPointIndicator != null && turnManager.CurrentCharacter != null)
        {
            actionPointIndicator.text = $"A/P: {turnManager.CurrentCharacter.currentActionPoints}";
        }
    }

    void UpdateTurnQueueUI()
    {
        var queue = turnManager.GetUpcomingTurns(5);
        string text = "Turn Order:\n";
        for (int i = 0; i < queue.Count; i++)
        {
            text += $"{i + 1}. {queue[i].characterName}\n";
        }


        // Assign to a TextMeshProUGUI component (you can expose it via [Header("UI")] section)
        turnsOrder.text = text;
    }

    public void ShowMoveRange(CharacterBase character, int range)
    {
        ClearHighlights();
        selectedCharacter = character;

        Vector2Int origin = character.gridPosition;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int target = origin + new Vector2Int(x, y);
                int distance = Mathf.Abs(x) + Mathf.Abs(y);

                if (distance <= range && IsWalkable(target))
                {
                    Vector3 highlightPos = new Vector3((target.x + 0.5f) * tileSize, (target.y - 0.5f) * tileSize, -1f);
                    GameObject highlight = Instantiate(moveHighlightPrefab, highlightPos, Quaternion.identity);
                    highlight.GetComponent<TileHighlight>().Init(target);
                    activeHighlights.Add(highlight);
                }
            }
        }
    }

    public void ClearHighlights()
    {
        foreach (GameObject highlight in activeHighlights)
            Destroy(highlight);

        activeHighlights.Clear();
    }

    public void TryMoveSelectedCharacter(Vector2Int target)
    {
        if (selectedCharacter == null) return;

        if (!IsWalkable(target)) return;

        ClearHighlights();
        selectedCharacter.MoveTo(target);
        selectedCharacter = null;
    }

    public void RemoveCharacter(CharacterBase character)
    {
        turnManager.RemoveFromTurnOrder(character);

        if (turnManager.TurnOrder.Count == 0)
        {
            Debug.Log("All characters are dead. Game Over!");
            gameOverScreen.SetActive(true);
        }
    }

    public bool IsWalkable(Vector2Int pos)
    {
        // Must be in floorPositions
        return dungeonGenerator.GetFloorPositions().Contains(pos) && !IsTileOccupied(pos);
    }

    public bool IsTileOccupied(Vector2Int pos)
    {
        foreach (var character in turnManager.TurnOrder)
        {
            if (character.gridPosition == pos)
                return true;
        }
        return false;
    }

    public void ShowAttackRange(CharacterBase character, int range)
    {
        ClearHighlights();
        selectedCharacter = character;

        Vector2Int origin = character.gridPosition;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int target = origin + new Vector2Int(x, y);
                int distance = Mathf.Abs(x) + Mathf.Abs(y);

                if (distance <= range && IsTileOccupied(target))
                {
                    Vector3 highlightPos = new Vector3(target.x + 0.5f, target.y + 0.5f, -1f);
                    GameObject highlight = Instantiate(moveHighlightPrefab, highlightPos, Quaternion.identity);
                    highlight.GetComponent<TileHighlight>().Init(target, true); // mark as attackable
                    activeHighlights.Add(highlight);
                }
            }
        }
    }

    public void TryAttackTarget(Vector2Int target)
    {
        ClearHighlights();

        CharacterBase attacker = selectedCharacter;
        CharacterBase defender = null;

        foreach (var character in turnManager.TurnOrder)
        {
            if (character.gridPosition == target && character != attacker)
            {
                defender = character;
                break;
            }
        }

        if (defender != null)
        {
            defender.TakeDamage(attacker.attackDamage);
            attacker.currentActionPoints--; // cost 1 AP to attack
        }

        selectedCharacter = null;
    }

}
