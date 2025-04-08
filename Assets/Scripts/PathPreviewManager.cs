using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PathPreviewManager : MonoBehaviour
{
    public GameObject previewTilePrefab;
    private List<GameObject> activePreviews = new List<GameObject>();

    public static PathPreviewManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void ShowPath(List<Vector2Int> path)
    {
        Clear();

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int to = path[i];
            Vector3 worldPos = GameManager.Instance.GridToWorld(to) + new Vector3(0.5f, -0.5f, 0f);
            GameObject previewTileActive = Instantiate(previewTilePrefab, worldPos, Quaternion.identity);

            /* var sprite = previewTileActive.GetComponentInChildren<SpriteRenderer>();
            if (sprite != null)
            {
                if (i == 1) sprite.color = Color.green;
                else if (i < 2) sprite.color = Color.yellow;
                else sprite.color = Color.red;
            } */

            TextMeshProUGUI actionPointsLabel = previewTileActive.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log($"[ROGUE]: {actionPointsLabel}, A/P: {i}");
            if (actionPointsLabel != null)
                actionPointsLabel.text = (i + 1).ToString() + "A/P";

            activePreviews.Add(previewTileActive);
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
