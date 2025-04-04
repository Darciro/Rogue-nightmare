using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI turnIndicator;
    public TextMeshProUGUI actionPointIndicator;

    [Header("Map Settings")]
    public int height = 10;
    public int width = 16;
    public float tileSize = 1f;
    [Header("Tile Prefabs")]
    public GameObject[] tilePrefabs;
    [Header("Highlighting")]
    public GameObject moveHighlightPrefab;

    [Header("Entities")]
    public GameObject[] enemyPrefabs;
    public GameObject[] pickupPrefabs;
    public GameObject[] wallPrefabs;

    private CharacterBase[] turnOrder;
    private int currentTurnIndex = 0;
    private Tile[,] tiles;

    private List<GameObject> activeHighlights = new List<GameObject>();

    private CharacterBase selectedCharacter;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        CenterCamera();
        GenerateMap();
        PlaceEntities();

        turnOrder = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
        StartTurns();
    }

    // ─────────────── GRID ───────────────

    void GenerateMap()
    {
        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(x * tileSize, y * tileSize, 0);
                GameObject randomPrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
                GameObject tileGO = Instantiate(randomPrefab, worldPos, Quaternion.identity);
                tileGO.transform.SetParent(transform);

                Tile tile = tileGO.GetComponent<Tile>();
                tile.Init(new Vector2Int(x, y), true);
                tiles[x, y] = tile;
            }
        }

        // CenterCamera();
    }

    void CenterCamera()
    {
        float cx = (width - 1) * tileSize / 2f;
        float cy = (height - 1) * tileSize / 2f - 0.5f;
        Camera.main.transform.position = new Vector3(cx, cy, -10f);
    }

    public bool IsWalkable(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width &&
               pos.y >= 0 && pos.y < height &&
               tiles[pos.x, pos.y].walkable &&
               !tiles[pos.x, pos.y].IsOccupied();
    }

    public Vector3 GridToWorld(Vector2Int gridPos) =>
        new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, 0);

    public Vector2Int WorldToGrid(Vector3 worldPos) =>
        new Vector2Int(Mathf.RoundToInt(worldPos.x / tileSize), Mathf.RoundToInt(worldPos.y / tileSize));

    public Tile GetTile(Vector2Int pos) => tiles[pos.x, pos.y];

    // ─────────────── ENTITY PLACEMENT ───────────────

    void PlaceEntities()
    {
        for (int i = 0; i < 2; i++) AddRandomEntity(enemyPrefabs);
        for (int i = 0; i < 2; i++) AddRandomEntity(pickupPrefabs);
        for (int i = 0; i < 5; i++) AddRandomEntity(wallPrefabs);
    }

    void AddRandomEntity(GameObject[] prefabs)
    {
        if (prefabs.Length == 0) return;

        Vector2Int pos = GetRandomFreeTile();
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        AddEntity(prefab, pos);
    }

    bool AddEntity(GameObject prefab, Vector2Int pos)
    {
        if (!IsWalkable(pos)) return false;

        Vector3 worldPos = GridToWorld(pos);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity);
        GetTile(pos).occupant = obj;

        // If it's a character, update its grid position
        CharacterBase cb = obj.GetComponent<CharacterBase>();
        if (cb != null) cb.gridPosition = pos;

        return true;
    }

    Vector2Int GetRandomFreeTile()
    {
        while (true)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector2Int pos = new Vector2Int(x, y);
            if (IsWalkable(pos)) return pos;
        }
    }

    // ─────────────── TURNS ───────────────

    void StartTurns()
    {
        currentTurnIndex = 0;
        turnOrder[currentTurnIndex].StartTurn();
        // UpdateTurnUI();
    }

    public void NextTurn()
    {
        currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Length;
        turnOrder[currentTurnIndex].StartTurn();
        // UpdateTurnUI();
    }

    void Update()
    {
        if (turnOrder != null && turnIndicator != null)
        {
            turnIndicator.text = $"{turnOrder[currentTurnIndex].characterName}'s Turn";
        }

        if (actionPointIndicator != null)
        {
            actionPointIndicator.text = $"A/P: {turnOrder[currentTurnIndex].currentActionPoints}";
        }
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
                    Vector3 highlightPos = new Vector3(target.x * tileSize + 0.5f, target.y * tileSize - 0.5f, -1f); // Note: Z = -1
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
        ClearHighlights();
        selectedCharacter.MoveTo(target);
        selectedCharacter = null;
    }

}
