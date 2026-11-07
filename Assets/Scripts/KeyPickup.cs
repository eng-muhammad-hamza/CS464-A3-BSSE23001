using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] Color color = JuiceFx.KeyColor;
    bool collected;

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        JuiceFx.Burst(transform.position, color, 26, 5f, 0.16f, 0.6f);
        JuiceFx.Ring(transform.position, color, 2.2f, 0.45f);
        if (GameManager.Instance != null) GameManager.Instance.AddKey();
        Destroy(gameObject);
    }
}