using TMPro;
using UnityEngine;

public class HintFade : MonoBehaviour
{
    [SerializeField] float showSeconds = 6f;
    [SerializeField] float fadeSeconds = 1f;
    TMP_Text label;
    float startAlpha = 1f;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        if (label != null) startAlpha = label.alpha;
    }

    void Update()
    {
        float t = Time.timeSinceLevelLoad;
        if (t > showSeconds + fadeSeconds)
        {
            gameObject.SetActive(false);
            return;
        }
        if (label != null && t > showSeconds)
            label.alpha = startAlpha * (1f - (t - showSeconds) / fadeSeconds);
    }
}
