using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    [SerializeField] int amount = 1;
    bool collected;

    void OnTriggerStay(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out Health health)) return;
        if (!health.Heal(amount)) return;

        collected = true;
        JuiceFx.Burst(transform.position, JuiceFx.HeartColor, 20, 4f, 0.16f, 0.5f);
        JuiceFx.Ring(transform.position, JuiceFx.HeartColor, 1.8f, 0.35f);
        Destroy(gameObject);
    }
}