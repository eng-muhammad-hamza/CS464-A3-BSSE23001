using System.Collections;
using UnityEngine;

public class CollapsingTile : MonoBehaviour
{
    [SerializeField] Transform visual;
    [SerializeField] Renderer visualRenderer;
    [SerializeField] Collider solid;
    [SerializeField] float crumbleSeconds = 0.9f;
    [SerializeField] float fallSeconds = 0.7f;
    [SerializeField] float respawnSeconds = 6f;

    static readonly Color WarnColor = new Color(1f, 0.45f, 0.1f);
    Material material;
    Color baseColor;
    Vector3 visualHome;
    Vector3 visualScale;
    bool busy;

    void Awake()
    {
        material = visualRenderer.material;
        baseColor = material.color;
        visualHome = visual.localPosition;
        visualScale = visual.localScale;
    }

    void OnCollisionEnter(Collision collision) => Touch(collision.collider);
    void OnCollisionStay(Collision collision) => Touch(collision.collider);

    void Touch(Collider other)
    {
        if (busy || !other.CompareTag("Player")) return;
        busy = true;
        StartCoroutine(Collapse());
    }

    IEnumerator Collapse()
    {
        float t = 0f;
        while (t < crumbleSeconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / crumbleSeconds);
            material.color = Color.Lerp(baseColor, WarnColor, k);
            Vector2 jitter = Random.insideUnitCircle * (0.05f * k);
            visual.localPosition = visualHome + new Vector3(jitter.x, 0f, jitter.y);
            yield return null;
        }
        visual.localPosition = visualHome;

        solid.enabled = false;
        ClearPickupsOnTile();
        JuiceFx.Burst(transform.position, new Color(0.6f, 0.58f, 0.55f), 10, 3f, 0.2f, 0.5f);

        t = 0f;
        float drop = 0f;
        float speed = 0f;
        while (t < fallSeconds)
        {
            t += Time.deltaTime;
            speed += 25f * Time.deltaTime;
            drop += speed * Time.deltaTime;
            visual.localPosition = visualHome + Vector3.down * drop;
            visual.localScale = visualScale * Mathf.Lerp(1f, 0.6f, t / fallSeconds);
            yield return null;
        }
        visual.gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnSeconds);
        visual.localPosition = visualHome;
        visual.localScale = Vector3.zero;
        material.color = baseColor;
        visual.gameObject.SetActive(true);
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            visual.localScale = visualScale * Mathf.Clamp01(t / 0.3f);
            yield return null;
        }
        visual.localScale = visualScale;
        solid.enabled = true;
        busy = false;
    }

    void ClearPickupsOnTile()
    {
        Collider[] hits = Physics.OverlapBox(transform.position + Vector3.up * 0.8f, new Vector3(0.95f, 0.8f, 0.95f),
                                             Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            Coin coin = hit.GetComponentInParent<Coin>();
            if (coin != null) { Destroy(coin.gameObject); continue; }
            PowerUp powerUp = hit.GetComponentInParent<PowerUp>();
            if (powerUp != null) { Destroy(powerUp.gameObject); continue; }
            HeartPickup heart = hit.GetComponentInParent<HeartPickup>();
            if (heart != null) Destroy(heart.gameObject);
        }
    }
}