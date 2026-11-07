using UnityEngine;

public class SpawnPopIn : MonoBehaviour
{
    [SerializeField] float popSeconds = 0.35f;
    [SerializeField] bool sparkleOnSpawn = true;
    Vector3 finalScale;
    float elapsedSeconds;

    void Awake()
    {
        finalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Start()
    {
        if (sparkleOnSpawn) JuiceFx.Burst(transform.position, Color.white, 8, 2f, 0.1f, 0.4f);
    }

    void Update()
    {
        elapsedSeconds += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedSeconds / popSeconds);
        transform.localScale = finalScale * EaseOutBack(progress);
        if (progress >= 1f)
        {
            transform.localScale = finalScale;
            enabled = false;
        }
    }

    static float EaseOutBack(float progress)
    {
        const float overshoot = 1.70158f;
        float shifted = progress - 1f;
        return 1f + (overshoot + 1f) * shifted * shifted * shifted + overshoot * shifted * shifted;
    }
}