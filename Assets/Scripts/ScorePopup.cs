using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    TextMeshPro label;
    float elapsed;
    const float Lifetime = 1.0f;

    public static void Spawn(int points, Vector3 worldPos, Color color)
    {
        var go = new GameObject("ScorePopup");
        go.transform.position = worldPos + Vector3.back * 0.5f;
        var popup = go.AddComponent<ScorePopup>();
        popup.label = go.AddComponent<TextMeshPro>();
        popup.label.text = $"+{points}";
        popup.label.fontSize = 5f;
        popup.label.fontStyle = FontStyles.Bold;
        popup.label.color = color;
        popup.label.alignment = TextAlignmentOptions.Center;
        popup.label.outlineWidth = 0.3f;
        popup.label.outlineColor = new Color32(0, 0, 0, 220);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += Vector3.up * 2.5f * Time.deltaTime;
        float alpha = Mathf.Clamp01(1f - elapsed / Lifetime);
        var c = label.color;
        c.a = alpha;
        label.color = c;
        if (elapsed >= Lifetime) Destroy(gameObject);
    }
}
