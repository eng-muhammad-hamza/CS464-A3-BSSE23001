using UnityEngine;

public class IceZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController pc)) pc.EnterIce();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController pc)) pc.ExitIce();
    }
}