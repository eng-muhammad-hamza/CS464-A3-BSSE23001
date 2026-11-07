using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] int value = 1;
    [SerializeField] float spinSpeed = 90f;
    bool alreadyCollected;

    void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

    void OnTriggerEnter(Collider other)
    {
        if (alreadyCollected || !other.CompareTag("Player")) return;
        alreadyCollected = true;

        JuiceFx.Burst(transform.position, JuiceFx.GoldColor, 14, 4f, 0.14f, 0.5f);
        JuiceFx.Ring(transform.position, JuiceFx.GoldColor, 1.3f, 0.3f);
        JuiceFx.FloatingText(transform.position + Vector3.up * 0.8f, $"+{value}", JuiceFx.GoldColor);

        if (GameManager.Instance != null) GameManager.Instance.AddScore(value);
        else Debug.LogError("No active GameManager in the scene.", this);
        Destroy(gameObject);
    }
}
