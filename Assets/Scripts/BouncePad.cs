using System.Collections;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] float launchSpeed = 8f;
    [SerializeField, Range(0f, 1f)] float keepSideways = 0.5f;
    [SerializeField] float cooldown = 0.3f;
    [SerializeField] Transform visual;

    float readyAt;
    Vector3 visualRest = Vector3.one;

    void Awake()
    {
        if (visual != null) visualRest = visual.localScale;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < readyAt || !other.CompareTag("Player")) return;
        Rigidbody body = other.attachedRigidbody;
        if (body == null) return;
        readyAt = Time.time + cooldown;

        Vector3 v = body.linearVelocity;
        body.linearVelocity = new Vector3(v.x * keepSideways, 0f, v.z * keepSideways);
        body.AddForce(transform.up * launchSpeed, ForceMode.VelocityChange);

        JuiceFx.Burst(transform.position, JuiceFx.SpringColor, 18, 4f, 0.16f, 0.5f);
        JuiceFx.Ring(transform.position + Vector3.up * 0.1f, JuiceFx.SpringColor, 2f, 0.35f);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 0.12f);
        if (visual != null)
        {
            StopAllCoroutines();
            StartCoroutine(Squash());
        }
    }

    IEnumerator Squash()
    {
        const float seconds = 0.3f;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / seconds * Mathf.PI);
            visual.localScale = new Vector3(visualRest.x * (1f + 0.25f * k), visualRest.y * (1f - 0.6f * k), visualRest.z * (1f + 0.25f * k));
            yield return null;
        }
        visual.localScale = visualRest;
    }
}
