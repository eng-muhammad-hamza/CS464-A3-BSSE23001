using UnityEngine;

[RequireComponent(typeof(CameraShake))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 16f, -9f);
    [SerializeField] float smoothing = 6f;
    [SerializeField] float lookAheadSeconds = 0.25f;
    [SerializeField] float maxLookAhead = 3f;
    [SerializeField] Vector2 minXZ = new Vector2(-1000f, -1000f);
    [SerializeField] Vector2 maxXZ = new Vector2(1000f, 1000f);

    CameraShake shake;
    Rigidbody targetBody;
    Vector3 lookAhead;

    void Awake() => shake = GetComponent<CameraShake>();

    void Start()
    {
        if (target == null) return;
        targetBody = target.GetComponent<Rigidbody>();
        shake.RestingLocalPosition = Desired();
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 wanted = Vector3.zero;
        if (targetBody != null)
        {
            Vector3 v = targetBody.linearVelocity;
            wanted = Vector3.ClampMagnitude(new Vector3(v.x, 0f, v.z) * lookAheadSeconds, maxLookAhead);
        }
        float blend = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
        lookAhead = Vector3.Lerp(lookAhead, wanted, blend);
        shake.RestingLocalPosition = Vector3.Lerp(shake.RestingLocalPosition, Desired(), blend);
    }

    Vector3 Desired()
    {
        Vector3 p = target.position + lookAhead + offset;
        p.x = Mathf.Clamp(p.x, minXZ.x, maxXZ.x);
        p.z = Mathf.Clamp(p.z, minXZ.y, maxXZ.y);
        return p;
    }
}
