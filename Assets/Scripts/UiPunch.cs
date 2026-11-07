using UnityEngine;

public class UiPunch : MonoBehaviour
{
    [SerializeField] float punchScale = 1.35f;
    [SerializeField] float punchSeconds = 0.25f;
    Vector3 restingScale;
    float elapsedSeconds;

    void Awake()
    {
        restingScale = transform.localScale;
        elapsedSeconds = punchSeconds;
    }

    public void Punch() => elapsedSeconds = 0f;

    void Update()
    {
        if (elapsedSeconds >= punchSeconds) return;
        elapsedSeconds += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(elapsedSeconds / punchSeconds);
        transform.localScale = restingScale * (1f + (punchScale - 1f) * Mathf.Sin(progress * Mathf.PI));
        if (progress >= 1f) transform.localScale = restingScale;
    }
}