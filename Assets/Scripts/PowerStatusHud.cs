using System.Text;
using TMPro;
using UnityEngine;

public class PowerStatusHud : MonoBehaviour
{
    [SerializeField] bool showSpeedReadout = false;
    TMP_Text label;
    PlayerController controller;
    readonly StringBuilder builder = new StringBuilder();

    void Awake() => label = GetComponent<TMP_Text>();
    void Start() => controller = FindAnyObjectByType<PlayerController>();

    void Update()
    {
        if (controller == null) return;
        builder.Clear();
        if (controller.IsSpeedBoosted)
            builder.AppendLine($"SPEED x{controller.SpeedMultiplier:0.0}  {controller.SpeedBoostSecondsRemaining:0.0}s");
        if (showSpeedReadout)
            builder.AppendLine($"{controller.FlatSpeed:0.0} m/s");
        label.text = builder.ToString();
    }
}
