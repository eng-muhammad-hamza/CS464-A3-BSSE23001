using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float knockback = 5f;
    [SerializeField] float knockTime = 0.2f;
    [SerializeField] float lifetimeSeconds = 4f;
    Vector3 direction;
    float speed;
    float age;
    bool launched;

    public void Launch(Vector3 aimDirection, float bulletSpeed)
    {
        direction = aimDirection.normalized;
        speed = bulletSpeed;
        launched = true;
    }

    void Update()
    {
        if (!launched) return;
        age += Time.deltaTime;
        transform.position += direction * (speed * Time.deltaTime);
        if (age >= lifetimeSeconds) Pop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!launched) return;
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out Health health) && health.TakeDamage(damage))
            {
                Vector3 flat = new Vector3(direction.x, 0f, direction.z).normalized;
                if (other.TryGetComponent(out PlayerController controller)) controller.Knockback(flat * knockback, knockTime);
            }
            Pop();
            return;
        }
        if (other.isTrigger || other.GetComponentInParent<Turret>() != null) return;
        Pop();
    }

    void Pop()
    {
        JuiceFx.Burst(transform.position, JuiceFx.DamageColor, 8, 3f, 0.1f, 0.3f);
        Destroy(gameObject);
    }
}