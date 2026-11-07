using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    public Vector3 RestingLocalPosition { get; set; }

    float shakeSecondsLeft;
    float shakeTotalSeconds;
    float shakeMagnitude;

    void Awake()
    {
        Instance = this;
        RestingLocalPosition = transform.localPosition;
    }

    public void Shake(float durationSeconds, float magnitude)
    {
        if (durationSeconds <= 0f || magnitude <= 0f) return;
        if (magnitude < CurrentShakeStrength()) return;
        shakeSecondsLeft = durationSeconds;
        shakeTotalSeconds = durationSeconds;
        shakeMagnitude = magnitude;
    }

    float CurrentShakeStrength() =>
        shakeTotalSeconds <= 0f ? 0f : shakeMagnitude * Mathf.Max(0f, shakeSecondsLeft / shakeTotalSeconds);

    void LateUpdate()
    {
        Vector3 shakeOffset = Vector3.zero;
        if (shakeSecondsLeft > 0f)
        {
            shakeSecondsLeft -= Time.unscaledDeltaTime;
            Vector2 randomOffset = Random.insideUnitCircle * CurrentShakeStrength();
            shakeOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
        }
        transform.localPosition = RestingLocalPosition + shakeOffset;
    }
}