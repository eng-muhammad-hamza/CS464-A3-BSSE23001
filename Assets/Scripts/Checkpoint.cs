using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Renderer flagRenderer;
    [SerializeField] int healAmount = 1;
    static readonly Color InactiveColor = new Color(0.55f, 0.58f, 0.65f);
    bool reached;

    void Start()
    {
        if (flagRenderer != null) flagRenderer.material.color = InactiveColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        if (GameManager.Instance == null) return;
        reached = true;

        GameManager.Instance.SetCheckpoint(transform);
        if (flagRenderer != null) flagRenderer.material.color = JuiceFx.PositiveColor;
        if (healAmount > 0 && other.TryGetComponent(out Health health)) health.Heal(healAmount);

        Vector3 center = transform.position + Vector3.up * 0.3f;
        JuiceFx.Ring(center, JuiceFx.PositiveColor, 4f, 0.6f);
        JuiceFx.Burst(center + Vector3.up, JuiceFx.PositiveColor, 30, 5f, 0.18f, 0.7f);
        JuiceFx.FloatingText(center + Vector3.up * 2f, "CHECKPOINT", JuiceFx.PositiveColor, 18f);
    }
}
