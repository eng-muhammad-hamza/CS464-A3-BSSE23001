using TMPro;
using UnityEngine;

public class WorldHint : MonoBehaviour
{
    [SerializeField] float showRadius = 7f;
    [SerializeField] float fadeSpeed = 3f;
    TMP_Text label;
    Transform player;
    float alpha;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
        if (label != null) label.alpha = 0f;
    }

    void Start()
    {
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (controller != null) player = controller.transform;
    }

    void Update()
    {
        if (player == null || label == null) return;
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        float wanted = d.magnitude < showRadius ? 1f : 0f;
        alpha = Mathf.MoveTowards(alpha, wanted, fadeSpeed * Time.unscaledDeltaTime);
        label.alpha = alpha;
    }
}
