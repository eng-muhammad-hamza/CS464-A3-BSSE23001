using UnityEngine;

public class TimePickup : MonoBehaviour
{
    [SerializeField] float seconds = 5f;
    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        Color c = JuiceFx.TimeColor;
        JuiceFx.Burst(transform.position, c, 24, 5f, 0.16f, 0.6f);
        JuiceFx.Ring(transform.position, c, 2f, 0.4f);
        JuiceFx.FloatingText(transform.position + Vector3.up * 0.8f, $"+{seconds:0}s", c, 16f);
        if (GameManager.Instance != null) GameManager.Instance.AddTime(seconds);
        Destroy(gameObject);
    }
}