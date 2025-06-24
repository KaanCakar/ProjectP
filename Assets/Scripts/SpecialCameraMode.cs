using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script manages a special camera mode that can be activated with a key press.
/// </summary>

public class SpecialCameraMode : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private KeyCode activationKey = KeyCode.C;
    [SerializeField] private Volume volumeEffect;

    [Header("Geçiş Hızları")]
    [SerializeField] private float increaseSpeed = 2.0f;
    [SerializeField] private float decreaseSpeed = 2.0f;

    [Header("Weight Limitleri")]
    [SerializeField] private float maxWeight = 1.0f;
    [SerializeField] private float minWeight = 0.0f;

    void Start()
    {
        if (volumeEffect == null)
        {
            Debug.LogError("Volume objesi atanmadı!");
            return;
        }
        volumeEffect.weight = minWeight;
    }

    void Update()
    {
        if (volumeEffect == null) return;
        if (Input.GetKey(activationKey))
        {
            volumeEffect.weight = Mathf.MoveTowards(volumeEffect.weight, maxWeight, increaseSpeed * Time.deltaTime);
        }

        else
        {
            volumeEffect.weight = Mathf.MoveTowards(volumeEffect.weight, minWeight, decreaseSpeed * Time.deltaTime);
        }
    }
}
