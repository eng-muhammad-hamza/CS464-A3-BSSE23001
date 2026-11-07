using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] Vector3 pushDirection = new Vector3(0f, 0f, -1f);
    [SerializeField] float pushForce = 5f;
    [SerializeField] float stripeLength = 1f;
    [SerializeField] float scrollSpeed = 2f;
    [SerializeField] bool swapAxes = false;
    [SerializeField] bool reverseScroll = false;
    [SerializeField] Color colorA = new Color(0.16f, 0.17f, 0.22f);
    [SerializeField] Color colorB = new Color(0.95f, 0.7f, 0.15f);

    Material material;
    Vector3 direction;
    bool alongX;
    bool tileOnU;
    Vector2 offset;

    void Start()
    {
        direction = pushDirection.normalized;
        alongX = Mathf.Abs(direction.x) > Mathf.Abs(direction.z);
        tileOnU = alongX != swapAxes;

        Texture2D texture = tileOnU
            ? new Texture2D(2, 1, TextureFormat.RGBA32, false)
            : new Texture2D(1, 2, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.SetPixel(0, 0, colorA);
        if (tileOnU) texture.SetPixel(1, 0, colorB); else texture.SetPixel(0, 1, colorB);
        texture.Apply();

        float length = alongX ? transform.localScale.x : transform.localScale.z;
        float repeats = length / (stripeLength * 2f);
        material = GetComponent<Renderer>().material;
        material.color = Color.white;
        material.mainTexture = texture;
        material.mainTextureScale = tileOnU ? new Vector2(repeats, 1f) : new Vector2(1f, repeats);
    }

    void Update()
    {
        float sign = alongX ? Mathf.Sign(direction.x) : Mathf.Sign(direction.z);
        float step = sign * (reverseScroll ? 1f : -1f) * scrollSpeed / (stripeLength * 2f) * Time.deltaTime;
        if (tileOnU) offset.x = Mathf.Repeat(offset.x + step, 1f);
        else offset.y = Mathf.Repeat(offset.y + step, 1f);
        material.mainTextureOffset = offset;
    }

    void OnTriggerStay(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        body.AddForce(direction * pushForce, ForceMode.Acceleration);
    }
}