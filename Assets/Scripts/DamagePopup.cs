using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float duration = 1f;
    public Vector3 floatOffset = new Vector3(0, 1.5f, 0);

    private TextMeshProUGUI text;
    private Color originalColor;
    private float timeElapsed;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalColor = text.color;
    }

    public void Setup(int damage)
    {
        text.text = damage.ToString();
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        transform.position += floatOffset * Time.deltaTime;
        float fade = 1f - (timeElapsed / duration);
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, fade);

        if (timeElapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
