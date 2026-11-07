using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spinner : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 60f;
    Rigidbody body;
    float angle;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        angle = transform.eulerAngles.y;
    }

    void FixedUpdate()
    {
        angle += degreesPerSecond * Time.fixedDeltaTime;
        body.MoveRotation(Quaternion.Euler(0f, angle, 0f));
    }
}
