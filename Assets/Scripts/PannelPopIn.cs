using UnityEngine;

public class PanelPopIn : MonoBehaviour
{
    [SerializeField] float popSeconds = 0.45f;
    Vector3 finalScale;
    float elapsedSeconds;

    void Awake() => finalScale = transform.localScale;

    void OnEnable()
    {
        elapsedSeconds = 0f;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (elapsedSeconds >= popSeconds) return;
        elapsedSeconds += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(elapsedSeconds / popSeconds);
        float shifted = progress - 1f;
        float eased = 1f + 2.70158f * shifted * shifted * shifted + 1.70158f * shifted * shifted;
        transform.localScale = finalScale * eased;
    }
}