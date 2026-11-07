using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] float multiplier = 1.6f;
    [SerializeField] float duration = 5f;
    bool alreadyCollected;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyCollected || !other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerController controller)) return;
        alreadyCollected = true;

        Color color = JuiceFx.SpeedColor;
        JuiceFx.Burst(transform.position, color, 30, 5f, 0.18f, 0.7f);
        JuiceFx.Ring(transform.position, color, 2.4f, 0.45f);
        JuiceFx.FloatingText(transform.position + Vector3.up * 0.9f, $"SPEED {duration:0}s", color, 14f);

        controller.BoostSpeed(multiplier, duration);
        Destroy(gameObject);
    }
}
