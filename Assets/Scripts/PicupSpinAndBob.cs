using UnityEngine;

public class PickupSpinAndBob : MonoBehaviour
{
    [SerializeField] float spinDegreesPerSecond = 90f;
    [SerializeField] float bobHeight = 0.15f;
    [SerializeField] float bobCyclesPerSecond = 0.6f;
    float startingHeight;

    void Start() => startingHeight = transform.position.y;

    void Update()
    {
        transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
        Vector3 currentPosition = transform.position;
        currentPosition.y = startingHeight + Mathf.Sin(Time.time * bobCyclesPerSecond * Mathf.PI * 2f) * bobHeight;
        transform.position = currentPosition;
    }
}