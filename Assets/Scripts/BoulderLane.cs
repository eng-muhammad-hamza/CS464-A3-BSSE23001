using System.Collections;
using UnityEngine;

public class BoulderLane : MonoBehaviour
{
    [SerializeField] Transform boulder;
    [SerializeField] Renderer stripRenderer;
    [SerializeField] float halfLength = 15f;
    [SerializeField] float rollSpeed = 9f;
    [SerializeField] float waitSeconds = 3.5f;
    [SerializeField] float warnSeconds = 1f;
    [SerializeField] float startDelay = 0f;
    [SerializeField] float boulderRadius = 1.1f;
    [SerializeField] Color idleColor = new Color(0.3f, 0.08f, 0.08f);
    [SerializeField] Color warnColor = new Color(1f, 0.2f, 0.2f);

    Material stripMaterial;

    IEnumerator Start()
    {
        stripMaterial = stripRenderer.material;

        Vector3 forward = transform.forward;
        Vector3 from = transform.position - forward * halfLength;
        Vector3 to = transform.position + forward * halfLength;
        from.y = transform.position.y + boulderRadius;
        to.y = transform.position.y + boulderRadius;
        float length = Vector3.Distance(from, to);

        boulder.gameObject.SetActive(false);
        stripMaterial.color = idleColor;
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            stripMaterial.color = idleColor;
            yield return new WaitForSeconds(waitSeconds);

            float t = 0f;
            while (t < warnSeconds)
            {
                stripMaterial.color = Mathf.FloorToInt(t * 10f) % 2 == 0 ? warnColor : idleColor;
                t += Time.deltaTime;
                yield return null;
            }
            stripMaterial.color = warnColor;

            boulder.position = from;
            boulder.gameObject.SetActive(true);
            float travelled = 0f;
            while (travelled < length)
            {
                float step = rollSpeed * Time.deltaTime;
                travelled += step;
                boulder.position = Vector3.Lerp(from, to, Mathf.Clamp01(travelled / length));
                boulder.Rotate(transform.right, step / boulderRadius * Mathf.Rad2Deg, Space.World);
                yield return null;
            }
            boulder.gameObject.SetActive(false);
        }
    }
}
