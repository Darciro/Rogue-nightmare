namespace RogueNightmare.Core
{
    using UnityEngine;
    using TMPro;
    using System.Collections.Generic;
    using RogueNightmare.Managers;
    using RogueNightmare.Characters;
    using RogueNightmare.Dungeon;
    using RogueNightmare.UI;
    
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI turnIndicatorText;
        [SerializeField] private TextMeshProUGUI turnOrderText;
        [SerializeField] private TextMeshProUGUI actionPointText;
        [SerializeField] private TextMeshProUGUI healthPointText;
        [SerializeField] private TextMeshProUGUI foodPointText;
        [SerializeField] private TextMeshProUGUI waterPointText;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] public GameObject damagePopupPrefab;
        
        [Header("Dungeon & Entity Prefabs")]
        [SerializeField] private DungeonManager dungeonManager;
        [SerializeField] private Transform dungeonParent;   // parent for dungeon tiles
        public Transform DungeonParent => dungeonParent;
        
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private GameObject[] pickupPrefabs;
        
        [Header("Highlight Prefabs")]
        [SerializeField] private GameObject moveHighlightPrefab;
        [SerializeField] private GameObject attackHighlightPrefab;
        
        [Header("Turn Management")]
        [SerializeField] public TurnManager turnManager;
        
        private List<GameObject> activeHighlights = new List<GameObject>();
        private CharacterManager selectedCharacter;
        public CharacterManager SelectedCharacter => selectedCharacter;

        void Awake()
        {
            // Singleton pattern enforcement
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
                return;
            }
            // Optionally, DontDestroyOnLoad(gameObject);
        }
        
        void Start()
        {
            // 1. Generate dungeon layout and instantiate dungeon GameObjects
            dungeonManager.GenerateDungeon();
            // 2. Spawn player and enemies into the dungeon
            SpawnEntities();
            // 3. Initialize turn order with all characters in scene
            CharacterManager[] characters = Object.FindObjectsByType<CharacterManager>(FindObjectsSortMode.None);
            turnManager.InitializeTurnOrder(new List<CharacterManager>(characters));
            // 4. Start the first character's turn
            turnManager.StartFirstTurn();
            // 5. Update UI for the first turn
            UpdateTurnUI();
        }
        
        private void SpawnEntities()
        {
            List<Vector2Int> openTiles = new List<Vector2Int>(dungeonManager.GetFloorPositions());
            // Spawn Player
            Vector2Int playerPos = GetRandomTile(openTiles);
            GameObject playerGO = Instantiate(playerPrefab, GridToWorld(playerPos), Quaternion.identity);
            var playerChar = playerGO.GetComponent<CharacterManager>();
            playerChar.gridPosition = playerPos;
            // Position the camera to follow the player
            Camera.main.GetComponent<CameraFollow>().SetTarget(playerGO.transform);
            // Spawn Enemies
            int enemyCount = 3;
            for (int i = 0; i < enemyCount; i++)
            {
                Vector2Int enemyPos = GetRandomTile(openTiles);
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject enemyGO = Instantiate(enemyPrefab, GridToWorld(enemyPos), Quaternion.identity);
                var enemyChar = enemyGO.GetComponent<CharacterManager>();
                enemyChar.gridPosition = enemyPos;
            }
            // (Pickups/traps could also be spawned here using pickupPrefabs if desired)
        }
        
        // Helper to get a random tile from the list and remove it (to avoid duplicates)
        private Vector2Int GetRandomTile(List<Vector2Int> tileList)
        {
            if (tileList.Count == 0) {
                Debug.LogWarning("No available tiles to place entity!");
                return Vector2Int.zero;
            }
            int index = Random.Range(0, tileList.Count);
            Vector2Int pos = tileList[index];
            tileList.RemoveAt(index);
            return pos;
        }
        
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            // Assuming each grid cell corresponds 1:1 to world units
            return new Vector3(gridPos.x, gridPos.y, 0f);
        }
        
        // Advances to the next turn (called by CharacterManager.EndTurn or UI).
        public void NextTurn()
        {
            // Clear any previews/highlights from previous turn
            PathPreviewManager.Instance?.Clear();
            ClearHighlights();
            turnManager.NextTurn();
            UpdateTurnUI();
        }
        
        public void RemoveCharacter(CharacterManager character)
        {
            turnManager.RemoveFromTurnOrder(character);
            if (turnManager.TurnOrder.Count == 0)
            {
                Debug.Log("All characters eliminated. Game Over.");
                ShowGameOver();
            }
        }
        
        public void UpdatePlayerUI()
        {
            // Update the player status display (HP, AP, hunger/thirst)
            if (turnManager.CurrentCharacter is PlayerCharacter player)
            {
                healthPointText.text = $"{player.currentHP}/{player.MaxHP}";
                actionPointText.text = $"{player.currentActionPoints}/{player.MaxActionPoints}";
                foodPointText.text = $"{player.foodPoints}/100";
                waterPointText.text = $"{player.waterPoints}/100";
            }
        }
        
        public void UpdateTurnUI()
        {
            // Update turn indicator text
            if (turnIndicatorText != null && turnManager.CurrentCharacter != null)
            {
                turnIndicatorText.text = $"{turnManager.CurrentCharacter.characterName}'s Turn";
            }
            // Update action points text (for current character)
            if (actionPointText != null && turnManager.CurrentCharacter != null)
            {
                // Show current vs max AP for player (others might not be needed to show)
                if (turnManager.CurrentCharacter is PlayerCharacter pc)
                    actionPointText.text = $"{pc.currentActionPoints}/{pc.MaxActionPoints}";
            }
            // Update the turn order queue display (next few turns)
            if (turnOrderText != null)
            {
                List<CharacterManager> queue = turnManager.GetUpcomingTurns(5);
                string orderStr = "Turn Order:\n";
                for (int i = 0; i < queue.Count; i++)
                {
                    orderStr += $"{i+1}. {queue[i].characterName}\n";
                }
                turnOrderText.text = orderStr;
            }
            // Update player-specific UI elements
            UpdatePlayerUI();
        }
        
        // Method to check if position is within dungeon boundaries
        public bool IsWithinBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < dungeonManager.DungeonWidth &&
                   position.y >= 0 && position.y < dungeonManager.DungeonHeight;
        }
        
        // Highlights all walkable tiles within `range` of the given character's position
        public void ShowMoveRange(CharacterManager character, int range)
        {
            ClearHighlights();
            selectedCharacter = character;
            Vector2Int origin = character.gridPosition;

            foreach (var pos in dungeonManager.GetFloorPositions())
            {
                int dist = Mathf.Abs(pos.x - origin.x) + Mathf.Abs(pos.y - origin.y);
                if (dist <= range && IsWalkable(pos))
                {
                    List<Vector2Int> path = FindPath(origin, pos, range);
                    if (path != null) 
                    {
                        Vector3 highlightWorld = new Vector3(pos.x, pos.y, -1f);

                        // Instantiate move highlight prefab:
                        GameObject highlight = Instantiate(moveHighlightPrefab, highlightWorld, Quaternion.identity);
                        TileHighlight tileHighlight = highlight.GetComponent<TileHighlight>();
                        tileHighlight.Init(pos, isAttack: false);

                        activeHighlights.Add(highlight);
                    }
                }
            }
        }
        
        public void ShowAttackRange(CharacterManager character, int range)
        {
            ClearHighlights();
            selectedCharacter = character;
            Vector2Int origin = character.gridPosition;

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    // Only allow straight lines (no diagonals), skip origin tile
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) == 0 || Mathf.Abs(dx) + Mathf.Abs(dy) > range) 
                        continue;
                    if (dx != 0 && dy != 0) 
                        continue;

                    Vector2Int target = origin + new Vector2Int(dx, dy);
                    if (IsWithinBounds(target))
                    {
                        Vector3 highlightWorld = new Vector3(target.x, target.y, -1f);

                        // Instantiate attack highlight prefab:
                        GameObject highlight = Instantiate(attackHighlightPrefab, highlightWorld, Quaternion.identity);
                        TileHighlight tileHighlight = highlight.GetComponent<TileHighlight>();
                        tileHighlight.Init(target, isAttack: true);

                        activeHighlights.Add(highlight);
                    }
                }
            }
        }
        
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, int maxRange)
        {
            // BFS pathfinding within maxRange distance
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();
            frontier.Enqueue(start);
            cameFrom[start] = start;
            costSoFar[start] = 0;
            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                if (current == goal) break;
                foreach (Vector2Int dir in new[]{ Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
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
            // Reconstruct path backwards from goal
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
        
        public Tile GetTile(Vector2Int pos) => dungeonManager.GetTile(pos);
        
        public bool IsWalkable(Vector2Int pos)
        {
            // A position is walkable if it's a floor and no character currently occupies it
            return dungeonManager.GetFloorPositions().Contains(pos) && !IsTileOccupied(pos);
        }
        
        public bool IsTileOccupied(Vector2Int pos)
        {
            // Check if any character in turn order is at this position
            foreach (var character in turnManager.TurnOrder)
            {
                if (character.gridPosition == pos) return true;
            }
            return false;
        }
        
        public void ClearHighlights()
        {
            foreach (GameObject hl in activeHighlights)
            {
                Destroy(hl);
            }
            activeHighlights.Clear();
        }
        
        public void TryMoveSelectedCharacter(Vector2Int target)
        {
            if (selectedCharacter == null) return;
            Vector2Int origin = selectedCharacter.gridPosition;
            int distance = Mathf.Abs(target.x - origin.x) + Mathf.Abs(target.y - origin.y);
            if (distance == 0 || distance > selectedCharacter.currentActionPoints) return;
            if (!IsWalkable(target)) return;
            selectedCharacter.MoveTo(target);
            selectedCharacter = null;
            ClearHighlights();
        }
        
        public void TryAttackTarget(Vector2Int target)
        {
            if (selectedCharacter == null) return;
            // Deduct an action point for initiating an attack
            selectedCharacter.UseActionPoints();
            UpdateTurnUI();
            ActionMenuUI.Instance.ShowActionButtons();
            if (selectedCharacter.currentActionPoints <= 0)
            {
                NextTurn();
                // Note: we return early if AP depleted, attack might not actually happen if target out of range.
            }
            Vector2Int origin = selectedCharacter.gridPosition;
            int distance = Mathf.Abs(target.x - origin.x) + Mathf.Abs(target.y - origin.y);
            if (distance == 0 || distance > selectedCharacter.attackRange) return;
            if (!(target.x == origin.x || target.y == origin.y)) return; // only straight lines
            // Find a defender at the target position
            CharacterManager defender = null;
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
            }
            ClearHighlights();
            selectedCharacter = null;
        }
        
        public void ShowGameOver()
        {
            gameOverScreen.SetActive(true);
            // Optionally freeze game or return to menu
        }
    }
}
