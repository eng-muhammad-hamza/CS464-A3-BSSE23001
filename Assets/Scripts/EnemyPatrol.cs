using UnityEngine;

public enum PatrolMode { Loop, PingPong }
public enum GuardState { Patrol, Chase, Return }

[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] Transform[] points;
    [SerializeField] PatrolMode mode = PatrolMode.Loop;
    [SerializeField] float speed = 2f;
    [SerializeField] float waitTime = 0f;
    [SerializeField] float turnSpeed = 300f;
    [SerializeField] float arriveDistance = 0.1f;

    [Header("Chase (chaseRange 0 = never chases)")]
    [SerializeField] float chaseSpeed = 3.5f;
    [SerializeField] float chaseRange = 6f;
    [SerializeField] float giveUpRange = 9f;

    [Header("Look (optional)")]
    [SerializeField] Renderer bodyRenderer;
    [SerializeField] Color patrolColor = new Color(0.55f, 0.1f, 0.1f);
    [SerializeField] Color chaseColor = new Color(1f, 0.25f, 0.15f);

    Rigidbody rb;
    Transform player;
    Health playerHealth;
    Material bodyMaterial;
    GuardState state = GuardState.Patrol;
    int index;
    int step = 1;
    float waitUntil;

    public GuardState State => state;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (bodyRenderer != null)
        {
            bodyMaterial = bodyRenderer.material;
            bodyMaterial.color = patrolColor;
        }
    }

    void Start()
    {
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller == null) return;
        player = controller.transform;
        playerHealth = controller.GetComponent<Health>();
    }

    void FixedUpdate()
    {
        if (points == null || points.Length == 0) return;
        UpdateState();

        switch (state)
        {
            case GuardState.Chase:
                MoveToward(player.position, chaseSpeed);
                break;
            case GuardState.Return:
                Vector3 home = points[index].position;
                if (Arrived(home)) SetState(GuardState.Patrol);
                else MoveToward(home, speed);
                break;
            default:
                Patrol();
                break;
        }
    }

    void UpdateState()
    {
        if (state == GuardState.Chase)
        {
            if (!InRange(giveUpRange)) SetState(GuardState.Return);
        }
        else if (InRange(chaseRange))
        {
            SetState(GuardState.Chase);
        }
    }

    void SetState(GuardState next)
    {
        if (next == state) return;
        state = next;
        Vector3 above = transform.position + Vector3.up * 2f;
        if (next == GuardState.Chase)
        {
            JuiceFx.FloatingText(above, "!", JuiceFx.DamageColor, 22f);
            JuiceFx.Ring(transform.position, JuiceFx.DamageColor, 2.5f, 0.35f);
            if (bodyMaterial != null) bodyMaterial.color = chaseColor;
        }
        else if (next == GuardState.Return)
        {
            JuiceFx.FloatingText(above, "?", JuiceFx.SlowColor, 18f);
        }
        if (next != GuardState.Chase && bodyMaterial != null) bodyMaterial.color = patrolColor;
    }

    void Patrol()
    {
        if (Time.time < waitUntil) return;
        Vector3 target = points[index].position;
        if (Arrived(target))
        {
            index = NextIndex();
            waitUntil = Time.time + waitTime;
            return;
        }
        MoveToward(target, speed);
    }

    bool Arrived(Vector3 target) => FlatDistance(rb.position, target) < arriveDistance;

    bool InRange(float range)
    {
        if (player == null || range <= 0f) return false;
        if (playerHealth != null && playerHealth.Current <= 0) return false;
        return FlatDistance(rb.position, player.position) < range;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void MoveToward(Vector3 target, float moveSpeed)
    {
        Vector3 from = rb.position;
        target.y = from.y;
        Vector3 direction = target - from;
        rb.MovePosition(Vector3.MoveTowards(from, target, moveSpeed * Time.fixedDeltaTime));
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, look, turnSpeed * Time.fixedDeltaTime));
        }
    }

    int NextIndex()
    {
        if (points.Length == 1) return 0;
        if (mode == PatrolMode.Loop) return (index + 1) % points.Length;
        if (index + step < 0 || index + step >= points.Length) step = -step;
        return index + step;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, giveUpRange);
    }
}
