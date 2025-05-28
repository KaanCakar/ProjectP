using UnityEngine;
using UnityEngine.Rendering;

public class SpecialCameraMode : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private KeyCode activationKey = KeyCode.C;
    [SerializeField] private Volume volumeEffect;
    
    [Header("Geçiş Hızları")]
    [SerializeField] private float increaseSpeed = 2.0f; // Weight artma hızı
    [SerializeField] private float decreaseSpeed = 2.0f; // Weight azalma hızı
    
    [Header("Weight Limitleri")]
    [SerializeField] private float maxWeight = 1.0f; // Maksimum weight değeri
    [SerializeField] private float minWeight = 0.0f; // Minimum weight değeri
    
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
