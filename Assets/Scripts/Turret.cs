using UnityEngine;

public class Turret : MonoBehaviour
{
    enum State { Sweep, Aim, Cooldown }

    [Header("Parts")]
    [SerializeField] Transform head;
    [SerializeField] Transform muzzle;
    [SerializeField] Transform aimBeam;
    [SerializeField] Renderer headRenderer;
    [SerializeField] Light sightLight;
    [SerializeField] GameObject bulletPrefab;

    [Header("Sweep")]
    [SerializeField] float baseYaw = 135f;
    [SerializeField] float sweepArc = 60f;
    [SerializeField] float secondsPerSweep = 6f;
    [SerializeField] float turnSpeed = 150f;
    [SerializeField] float startDelay = 3f;

    [Header("Sight and fire")]
    [SerializeField] float range = 16f;
    [SerializeField, Range(10f, 60f)] float halfAngle = 25f;
    [SerializeField] float aimSeconds = 0.8f;
    [SerializeField] float cooldownSeconds = 1.8f;
    [SerializeField] float bulletSpeed = 9f;

    static readonly Color IdleColor = new Color(0.35f, 0.38f, 0.46f);
    static readonly Color AimColor = new Color(1f, 0.25f, 0.2f);
    static readonly Color SightWarm = new Color(1f, 0.95f, 0.7f);

    Material headMaterial;
    Transform player;
    Health playerHealth;
    State state;
    float phase;
    float armedAt;
    float stateEndTime;

    void Awake()
    {
        if (headRenderer != null) headMaterial = headRenderer.material;
    }

    void Start()
    {
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null)
        {
            player = controller.transform;
            playerHealth = controller.GetComponent<Health>();
        }
        head.rotation = Quaternion.Euler(0f, baseYaw, 0f);
        armedAt = Time.time + startDelay;
        if (aimBeam != null) aimBeam.gameObject.SetActive(false);
        SetLook(IdleColor, SightWarm);
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsFinished) return;

        if (Time.time < armedAt) return;

        float yaw;
        if (state == State.Aim)
        {
            yaw = YawTo(player.position);
            if (!CanSee()) BackToSweep();
            else if (Time.time >= stateEndTime)
            {
                Fire();
                state = State.Cooldown;
                stateEndTime = Time.time + cooldownSeconds;
                if (aimBeam != null) aimBeam.gameObject.SetActive(false);
                SetLook(IdleColor, SightWarm);
            }
            else UpdateBeam();
        }
        else
        {
            phase += Time.deltaTime;
            yaw = baseYaw + Mathf.Sin(phase / Mathf.Max(0.5f, secondsPerSweep) * Mathf.PI * 2f) * sweepArc;
            if (state == State.Cooldown)
            {
                if (Time.time >= stateEndTime) state = State.Sweep;
            }
            else if (CanSee())
            {
                state = State.Aim;
                stateEndTime = Time.time + aimSeconds;
                SetLook(AimColor, AimColor);
                if (aimBeam != null) aimBeam.gameObject.SetActive(true);
            }
        }

        head.rotation = Quaternion.RotateTowards(head.rotation, Quaternion.Euler(0f, yaw, 0f), turnSpeed * Time.deltaTime);
    }

    void BackToSweep()
    {
        state = State.Sweep;
        if (aimBeam != null) aimBeam.gameObject.SetActive(false);
        SetLook(IdleColor, SightWarm);
    }

    bool CanSee()
    {
        if (playerHealth.Current <= 0) return false;
        Vector3 origin = muzzle.position;
        Vector3 target = player.position;
        target.y = origin.y;
        Vector3 toPlayer = target - origin;
        float distance = toPlayer.magnitude;
        if (distance > range) return false;
        if (distance < 0.01f) return true;
        if (Vector3.Angle(head.forward, toPlayer) > halfAngle) return false;
        if (Physics.Raycast(origin, toPlayer / distance, out RaycastHit hit, distance + 0.1f, ~0, QueryTriggerInteraction.Ignore))
            return hit.collider.CompareTag("Player");
        return false;
    }

    void Fire()
    {
        if (bulletPrefab == null) return;
        Vector3 target = player.position;
        target.y = muzzle.position.y;
        Vector3 direction = (target - muzzle.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(direction));
        if (bullet.TryGetComponent(out Projectile projectile)) projectile.Launch(direction, bulletSpeed);
        JuiceFx.Burst(muzzle.position, JuiceFx.DamageColor, 6, 3f, 0.1f, 0.25f);
    }

    void UpdateBeam()
    {
        if (aimBeam == null) return;
        Vector3 flat = player.position - muzzle.position;
        flat.y = 0f;
        float distance = Mathf.Min(flat.magnitude, range);
        float startZ = head.InverseTransformPoint(muzzle.position).z;
        aimBeam.localPosition = new Vector3(0f, 0f, startZ + distance * 0.5f);
        aimBeam.localScale = new Vector3(aimBeam.localScale.x, aimBeam.localScale.y, distance);
    }

    float YawTo(Vector3 point)
    {
        Vector3 d = point - transform.position;
        return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
    }

    void SetLook(Color body, Color beam)
    {
        if (headMaterial != null) headMaterial.color = body;
        if (sightLight != null) sightLight.color = beam;
    }
}