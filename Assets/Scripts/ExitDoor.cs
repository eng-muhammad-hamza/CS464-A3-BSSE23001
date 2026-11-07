using System.Collections;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [SerializeField] Transform gate;
    [SerializeField] Renderer gateRenderer;
    [SerializeField] Collider gateCollider;
    [SerializeField] float sinkDistance = 2f;
    [SerializeField] float openSeconds = 0.8f;
    bool opened;

    void Start()
    {
        if (gateRenderer != null) gateRenderer.material.color = JuiceFx.DamageColor;
    }

    void Update()
    {
        if (opened || GameManager.Instance == null || !GameManager.Instance.ExitUnlocked) return;
        opened = true;
        StartCoroutine(Open());
    }

    IEnumerator Open()
    {
        if (gateCollider != null) gateCollider.enabled = false;
        if (gateRenderer != null) gateRenderer.material.color = JuiceFx.PositiveColor;
        JuiceFx.Ring(transform.position + Vector3.up * 0.1f, JuiceFx.PositiveColor, 3f, 0.6f);

        Vector3 from = gate.position;
        Vector3 to = from + Vector3.down * sinkDistance;
        float elapsed = 0f;
        while (elapsed < openSeconds)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / openSeconds);
            gate.position = Vector3.Lerp(from, to, u * u * (3f - 2f * u));
            yield return null;
        }
        gate.gameObject.SetActive(false);
    }
}