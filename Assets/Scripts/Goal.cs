using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] float pulseInterval = 1.2f;
    float nextPulse;
    bool reached;

    void Update()
    {
        if (Time.time < nextPulse) return;
        nextPulse = Time.time + pulseInterval;
        bool open = GameManager.Instance != null && GameManager.Instance.GoalOpen;
        JuiceFx.Ring(transform.position + Vector3.up * 0.1f, open ? JuiceFx.PositiveColor : JuiceFx.DamageColor, 2.2f, 0.9f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.ReachGoal()) reached = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.GoalOpen && GameManager.Instance.ReachGoal()) reached = true;
    }
}
