using UnityEngine;

public class SpawnBlocker : MonoBehaviour
{
    public bool Covers(Vector3 worldPosition, float clearanceMeters)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        Vector3 worldScale = transform.lossyScale;
        float halfWidthWorld = Mathf.Abs(worldScale.x) * 0.5f + clearanceMeters;
        float halfDepthWorld = Mathf.Abs(worldScale.z) * 0.5f + clearanceMeters;
        float offsetAlongWidthWorld = Mathf.Abs(localPosition.x * worldScale.x);
        float offsetAlongDepthWorld = Mathf.Abs(localPosition.z * worldScale.z);
        return offsetAlongWidthWorld <= halfWidthWorld && offsetAlongDepthWorld <= halfDepthWorld;
    }
}