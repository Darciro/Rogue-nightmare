using RogueNightmare.Core;

namespace RogueNightmare.Managers
{
    using UnityEngine;
    using System.Collections.Generic;
    using TMPro;
    
    public class PathPreviewManager : MonoBehaviour
    {
        public static PathPreviewManager Instance { get; private set; }
        
        [SerializeField] private GameObject previewTilePrefab;
        private readonly List<GameObject> activePreviews = new List<GameObject>();
        
        void Awake()
        {
            Instance = this;
        }
        
        public void ShowPath(List<Vector2Int> path)
        {
            Clear();
            if (path == null) return;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 worldPos = GameManager.Instance.GridToWorld(path[i]);
                GameObject previewTile = Instantiate(previewTilePrefab, worldPos, Quaternion.identity, this.transform);
                // Optionally color-code the preview tile based on index (e.g., first step green, etc.)
                TextMeshProUGUI apText = previewTile.GetComponentInChildren<TextMeshProUGUI>();
                if (apText != null)
                {
                    apText.text = $"{i+1} AP";
                }
                activePreviews.Add(previewTile);
            }
        }
        
        public void Clear()
        {
            foreach (GameObject tile in activePreviews)
            {
                Destroy(tile);
            }
            activePreviews.Clear();
        }
    }
}