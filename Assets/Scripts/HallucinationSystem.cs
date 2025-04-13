using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class HallucinationSystem : MonoBehaviour
{
    [Header("Sanrı Parametreleri")]
    public float hallucinationLevel = 0f;
    public float maxHallucinationLevel = 100f;
    public float hallucinationDecreaseRate = 5f; 
    public float thresholdForControlChange = 70f;
    
    [Header("Bakış Parametreleri")]
    public float stareThreshold = 3f;
    public float detectionDistance = 5f;
    
    [Header("Sanrı Artıran Objeler")]
    public LayerMask hallucinogenicLayer;
    
    [Header("Post-Processing Efekleri")]
    public Volume postProcessingVolume;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private Bloom bloom;
    
    [Header("Kontrol Referansları")]
    public FPSController fpsController;
    
    [Header("Sanrı Kontrol Etkileri")]
    public float maxSensitivityMultiplier = 1.5f;
    public float maxSpeedReduction = 0.6f;
    
    [Header("Hareket Randomlaştırma")]
    public float randomMovementThreshold = 70f;
    public float maxRandomMovementStrength = 0.7f; 
    public float randomMovementFrequency = 2f;
    public float randomMovementDuration = 0.3f; 
    
    private Camera playerCamera;
    private float stareTime = 0f;
    private GameObject lastObjectStared = null;

    private float originalSensitivity;
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    
    private float modifiedSensitivity;
    private float modifiedWalkSpeed;
    private float modifiedSprintSpeed;
    
    private bool controlsAffected = false;
    
    private Vector2 randomMovementDirection;
    private float randomMovementTimer;
    private float nextRandomTime;
    private bool isRandomizing = false;

    public Text debugText;
    
    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.Log("Player kamera bulunamadı, ana kamera kullanılıyor.");
        }

        if (fpsController == null)
        {
            fpsController = GetComponent<FPSController>();
            if (fpsController == null)
            {
                Debug.LogError("FPSController bulunamadı! Kontrol değişimleri çalışmayacak.");
            }
        }
        
        if (fpsController != null)
        {
            originalSensitivity = fpsController.mouseSensitivity;
            originalWalkSpeed = fpsController.walkSpeed;
            originalSprintSpeed = fpsController.sprintSpeed;
        }

        if (postProcessingVolume != null)
        {
            postProcessingVolume.profile.TryGet(out vignette);
            postProcessingVolume.profile.TryGet(out chromaticAberration);
            postProcessingVolume.profile.TryGet(out bloom);
            
            ResetPostProcessingEffects();
        }
        else
        {
            Debug.LogWarning("Post Processing Volume atanmamış! Sanrı görsel efektleri çalışmayacak.");
        }

        randomMovementDirection = Vector2.zero;
        randomMovementTimer = 0f;
        nextRandomTime = 0f;
    }
    
    private void Update()
    {
        CheckForHallucinogenicObjects();
        UpdateHallucinationLevel();
        UpdateVisualEffects();
        HandleControlChanges();
        UpdateRandomMovement();
        
        if (debugText != null)
        {
            string controlStatus = controlsAffected ? 
                $"Kontroller Değişti (Hız:{modifiedWalkSpeed:F1}, Sens:{modifiedSensitivity:F1})" : "Normal";
            
            string randomStatus = isRandomizing ? 
                $"Aktif ({randomMovementDirection.x:F2}, {randomMovementDirection.y:F2})" : "Pasif";
                
            debugText.text = $"Sanrı: {hallucinationLevel:F1}/{maxHallucinationLevel}\n" +
                             $"Bakış: {stareTime:F1}/{stareThreshold}\n" +
                             $"Kontroller: {controlStatus}\n" +
                             $"Rastgele Hareket: {randomStatus}";
        }
    }
    
    private void CheckForHallucinogenicObjects()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, detectionDistance, hallucinogenicLayer))
        {
            HallucinogenicObject hallucinogenicObject = hit.collider.GetComponent<HallucinogenicObject>();
            
            if (hallucinogenicObject != null)
            {
                if (lastObjectStared == hit.collider.gameObject)
                {
                    stareTime += Time.deltaTime;
                    
                    if (stareTime >= stareThreshold)
                    {
                        hallucinationLevel += hallucinogenicObject.hallucinationIncrease * Time.deltaTime;
                        hallucinationLevel = Mathf.Clamp(hallucinationLevel, 0f, maxHallucinationLevel);
                    }
                }
                else
                {
                    lastObjectStared = hit.collider.gameObject;
                    stareTime = 0f;
                }
            }
        }
        else
        {
            lastObjectStared = null;
            stareTime = 0f;
        }
    }
    
    private void UpdateHallucinationLevel()
    {
        if (lastObjectStared == null)
        {
            hallucinationLevel -= hallucinationDecreaseRate * Time.deltaTime;
            hallucinationLevel = Mathf.Clamp(hallucinationLevel, 0f, maxHallucinationLevel);
        }
    }
    
    private void UpdateVisualEffects()
    {
        if (postProcessingVolume == null) return;
        
        float hallucinationRatio = hallucinationLevel / maxHallucinationLevel;
        
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(0.2f, 0.5f, hallucinationRatio);
            vignette.color.value = Color.Lerp(Color.black, Color.red, hallucinationRatio);
        }
        
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, hallucinationRatio);
        }

        if (bloom != null)
        {
            bloom.intensity.value = Mathf.Lerp(1f, 10f, hallucinationRatio);
            bloom.threshold.value = Mathf.Lerp(1f, 0.5f, hallucinationRatio);
        }
        
        if (hallucinationRatio > 0.7f)
        {
            float shakeAmount = Mathf.Lerp(0, 0.03f, (hallucinationRatio - 0.7f) / 0.3f);
            ShakeCamera(shakeAmount);
        }
    }
    
    private void ShakeCamera(float amount)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        Vector3 randomOffset = Random.insideUnitSphere * amount;
        playerCamera.transform.localPosition = originalPos + randomOffset;
    }
    
    private void HandleControlChanges()
    {
        if (fpsController == null) return;
        
        float hallucinationRatio = hallucinationLevel / maxHallucinationLevel;
        
        if (hallucinationLevel >= thresholdForControlChange)
        {
            float affectRatio = (hallucinationLevel - thresholdForControlChange) / 
                               (maxHallucinationLevel - thresholdForControlChange);
            
            modifiedSensitivity = originalSensitivity * (1f + (maxSensitivityMultiplier - 1f) * affectRatio);
            
            modifiedWalkSpeed = originalWalkSpeed * (1f - maxSpeedReduction * affectRatio);
            modifiedSprintSpeed = originalSprintSpeed * (1f - maxSpeedReduction * affectRatio);
            
            fpsController.mouseSensitivity = modifiedSensitivity;
            fpsController.walkSpeed = modifiedWalkSpeed;
            fpsController.sprintSpeed = modifiedSprintSpeed;
            
            controlsAffected = true;
        }
        else
        {
            fpsController.mouseSensitivity = originalSensitivity;
            fpsController.walkSpeed = originalWalkSpeed;
            fpsController.sprintSpeed = originalSprintSpeed;
            
            controlsAffected = false;
        }
    }
    
    private void UpdateRandomMovement()
    {
        if (hallucinationLevel >= randomMovementThreshold)
        {
            float randomStrength = Mathf.Lerp(0, maxRandomMovementStrength, 
                                  (hallucinationLevel - randomMovementThreshold) / 
                                  (maxHallucinationLevel - randomMovementThreshold));
            randomMovementTimer -= Time.deltaTime;
            
            if (randomMovementTimer <= 0)
            {
                if (isRandomizing)
                {
                    randomMovementDirection = Vector2.zero;
                    isRandomizing = false;
                    
                    nextRandomTime = Random.Range(1f, 3f) / Mathf.Lerp(1f, 2f, randomStrength);
                    randomMovementTimer = nextRandomTime;
                }
                else
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    randomMovementDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * randomStrength;
                    isRandomizing = true;
                    
                    randomMovementTimer = randomMovementDuration * Mathf.Lerp(0.5f, 1.5f, Random.value);
                }
            }
        }
        else
        {
            randomMovementDirection = Vector2.zero;
            isRandomizing = false;
        }
    }
    
    public Vector2 GetHallucinationMovementOffset()
    {
        return randomMovementDirection;
    }
    
    private void ResetPostProcessingEffects()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.2f;
            vignette.color.value = Color.black;
        }
        
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
        }
        
        if (bloom != null)
        {
            bloom.intensity.value = 1f;
            bloom.threshold.value = 1f;
        }
    }
}