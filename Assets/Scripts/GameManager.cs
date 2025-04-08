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

    [Header("Player UI")]
    public TextMeshProUGUI healthPointText;
    public TextMeshProUGUI foodPointText;
    public TextMeshProUGUI waterPointText;

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

    public CharacterBase SelectedCharacter => selectedCharacter;

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

    public Vector3 GridToWorld(Vector2Int pos)
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
        PathPreviewManager.Instance?.Clear();
        turnManager.NextTurn();
        UpdateTurnUI();
    }

    public void UpdatePlayerUI()
    {
        // Get the current player
        var player = turnManager.CurrentCharacter as PlayerCharacter;
        if (player == null) return;

        healthPointText.text = $"{player.currentHP}/{player.maxHP}";
        actionPointIndicator.text = $"{player.currentActionPoints}/{player.maxActionPoints}";
        foodPointText.text = $"{player.foodPoints}/100";
        waterPointText.text = $"{player.waterPoints}/100";
    }


    public void UpdateTurnUI()
    {
        if (turnIndicator != null && turnManager.CurrentCharacter != null)
        {
            turnIndicator.text = $"{turnManager.CurrentCharacter.characterName}'s Turn";
        }

        if (actionPointIndicator != null && turnManager.CurrentCharacter != null)
        {
            actionPointIndicator.text = $"A/P: {turnManager.CurrentCharacter.currentActionPoints}/3";
        }

        UpdatePlayerUI(); // ✅ Add this
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
                    var path = FindPath(origin, target, range);
                    if (path != null)
                    {
                        Vector3 highlightPos = new Vector3(target.x + 0.5f, target.y - 0.5f, -1f);
                        GameObject highlight = Instantiate(moveHighlightPrefab, highlightPos, Quaternion.identity);
                        highlight.GetComponent<TileHighlight>().Init(target);
                        activeHighlights.Add(highlight);
                    }
                }

            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int maxRange)
    {
        Queue<Vector2Int> frontier = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, int> costSoFar = new();

        frontier.Enqueue(start);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            foreach (Vector2Int dir in new[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        })
            {
                Vector2Int next = current + dir;
                int newCost = costSoFar[current] + 1;

                if (newCost > maxRange) continue;
                if (!IsWalkable(next) && next != goal) continue;

                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                    costSoFar[next] = newCost;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal)) return null;

        List<Vector2Int> path = new();
        Vector2Int step = goal;

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }


    public List<Vector2Int> OLD(Vector2Int start, Vector2Int goal, int maxRange)
    {
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        frontier.Enqueue(start);
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == goal) break;

            foreach (var dir in new[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        })
            {
                Vector2Int next = current + dir;

                int distance = Mathf.Abs(next.x - start.x) + Mathf.Abs(next.y - start.y);
                if (distance > maxRange || cameFrom.ContainsKey(next)) continue;

                if (IsWalkable(next) || next == goal)
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        // Reconstruct path
        if (!cameFrom.ContainsKey(goal)) return null;

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = goal;

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }

    public Tile GetTile(Vector2Int pos)
    {
        return dungeonGenerator.GetTile(pos);
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

        Vector2Int origin = selectedCharacter.gridPosition;
        int distance = Mathf.Abs(target.x - origin.x) + Mathf.Abs(target.y - origin.y);

        if (distance > selectedCharacter.currentActionPoints) return;
        if (distance == 0) return;
        if (!IsWalkable(target)) return;

        selectedCharacter.MoveTo(target);
        selectedCharacter = null;

        ClearHighlights();
    }

    public void RemoveCharacter(CharacterBase character)
    {
        turnManager.RemoveFromTurnOrder(character);

        if (turnManager.TurnOrder.Count == 0)
        {
            Debug.Log("ROGUE: All characters are dead. Game Over!");
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

                // Only cross pattern (no diagonals), no self
                if (distance > 0 && distance <= range && (x == 0 || y == 0))
                {
                    Vector3 highlightPos = new Vector3(target.x + 0.5f, target.y - 0.5f, -1f);

                    GameObject highlight = Instantiate(moveHighlightPrefab, highlightPos, Quaternion.identity);
                    highlight.GetComponent<TileHighlight>().Init(target, true); // true = attack
                    activeHighlights.Add(highlight);
                }
            }
        }
    }

    public void TryAttackTarget(Vector2Int target)
    {
        if (selectedCharacter != null)
        {
            selectedCharacter.currentActionPoints--;
            ActionMenuUI.Instance.ShowActionButtons();

            if (selectedCharacter.currentActionPoints <= 0)
            {
                GameManager.Instance.NextTurn();
            }
        }

        Vector2Int origin = selectedCharacter.gridPosition;
        int distance = Mathf.Abs(target.x - origin.x) + Mathf.Abs(target.y - origin.y);

        // Valid attack range, only in cross shape
        if (distance == 0 || distance > selectedCharacter.attackRange ||
            !(target.x == origin.x || target.y == origin.y)) return;

        // Look for character on the tile
        CharacterBase defender = null;
        foreach (var c in turnManager.TurnOrder)
        {
            if (c.gridPosition == target && c != selectedCharacter)
            {
                defender = c;
                break;
            }
        }

        if (defender != null)
        {
            defender.TakeDamage(selectedCharacter.attackDamage);
            selectedCharacter.currentActionPoints--;
        }

        ClearHighlights();
        selectedCharacter = null;
    }

}
