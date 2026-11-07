using UnityEngine;

public class CrusherTracker : MonoBehaviour
{
    [Header("Tracking")]
    [SerializeField] float trackSpeed = 2.2f;
    [SerializeField] Vector2 minXZ = new Vector2(-10.5f, -10.5f);
    [SerializeField] Vector2 maxXZ = new Vector2(10.5f, 10.5f);

    Hazard crusher;
    Transform player;
    Health playerHealth;
    float height;

    void Start()
    {
        height = transform.position.y;
        crusher = GetComponentInChildren<Hazard>();
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null)
        {
            player = controller.transform;
            playerHealth = controller.GetComponent<Health>();
        }
        else Debug.LogWarning("CrusherTracker: no PlayerController found.");
    }

    void Update()
    {
        if (player == null || crusher == null) return;
        if (!crusher.IsResting) return;
        if (playerHealth != null && playerHealth.Current <= 0) return;

        Vector3 target = new Vector3(
            Mathf.Clamp(player.position.x, minXZ.x, maxXZ.x),
            height,
            Mathf.Clamp(player.position.z, minXZ.y, maxXZ.y));
        transform.position = Vector3.MoveTowards(transform.position, target, trackSpeed * Time.deltaTime);
    }
}
