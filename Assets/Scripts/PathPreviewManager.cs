using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PathPreviewManager : MonoBehaviour
{
    public GameObject arrowPrefab;
    private List<GameObject> activeArrows = new List<GameObject>();

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
            Vector2Int from = i > 0 ? path[i - 1] : path[0];
            Vector2Int to = path[i];
            Vector2 dir = (to - from);

            Vector3 worldPos = GameManager.Instance.GridToWorld(to) + new Vector3(0.5f, -0.5f, 0f);
            GameObject arrow = Instantiate(arrowPrefab, worldPos, Quaternion.identity);

            // Rotate arrow based on direction
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle - 90); // Assumes up-facing arrow sprite

            // Show AP cost
            var label = arrow.GetComponentInChildren<TextMeshPro>();
            if (label != null)
                label.text = (i + 1).ToString(); // AP cost = tile number

            activeArrows.Add(arrow);
        }
    }

    public void Clear()
    {
        foreach (GameObject arrow in activeArrows)
        {
            Destroy(arrow);
        }
        activeArrows.Clear();
    }
}
