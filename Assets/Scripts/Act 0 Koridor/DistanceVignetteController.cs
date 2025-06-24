using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DistanceVignetteController : MonoBehaviour
{
    /// <summary>
    /// Kaan ÇAKAR - 2025
    /// DistanceVignetteController manages vignette effects based on the player's distance from specified points.
    /// </summary>


    [Header("Distance Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    
    [Header("Player Reference")]
    [SerializeField] private Transform player;
    
    [Header("Vignette Settings")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float minVignetteIntensity = 0.2f;
    [SerializeField] private float maxVignetteIntensity = 0.7f;
    [SerializeField] private AnimationCurve vignetteCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Activation")]
    [SerializeField] private bool activateOnStart = false;
    [SerializeField] private bool requireTrigger = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private Vignette vignette;
    private bool isActive = false;
    private float totalDistance;
    private float currentProgress;
    
    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
            
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out vignette);
            if (vignette != null)
            {
                vignette.intensity.value = minVignetteIntensity;
            }
        }
        
        if (startPoint != null && endPoint != null)
        {
            totalDistance = Vector3.Distance(startPoint.position, endPoint.position);
        }
        
        if (activateOnStart)
        {
            Activate();
        }
        
        // Trigger zone
        if (requireTrigger && startPoint != null)
        {
            GameObject triggerZone = new GameObject("VignetteStartTrigger");
            triggerZone.transform.position = startPoint.position;
            triggerZone.transform.parent = this.transform;
            
            BoxCollider col = triggerZone.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(5f, 3f, 5f);
            
            VignetteTriggerZone trigger = triggerZone.AddComponent<VignetteTriggerZone>();
            trigger.controller = this;
        }
    }
    
    private void Update()
    {
        if (!isActive || vignette == null || player == null) return;
        if (startPoint == null || endPoint == null) return;
        
        float playerDistanceFromStart = Vector3.Distance(player.position, startPoint.position);
        
        currentProgress = Mathf.Clamp01(playerDistanceFromStart / totalDistance);
        
        float distanceToEnd = Vector3.Distance(player.position, endPoint.position);
        if (distanceToEnd < playerDistanceFromStart && currentProgress > 0.9f)
        {
            currentProgress = 1f;
        }
        
        // Calculate vignette intensity based on the curve
        float curveValue = vignetteCurve.Evaluate(currentProgress);
        float targetIntensity = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, curveValue);
        
        // Smooth transition
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * 2f);
    }
    
    public void Activate()
    {
        isActive = true;
    }
    
    public void Deactivate()
    {
        isActive = false;
        if (vignette != null)
        {
            StartCoroutine(FadeVignetteToMin());
        }
    }
    
    private System.Collections.IEnumerator FadeVignetteToMin()
    {
        float startIntensity = vignette.intensity.value;
        float elapsed = 0f;
        float fadeTime = 2f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            vignette.intensity.value = Mathf.Lerp(startIntensity, minVignetteIntensity, t);
            yield return null;
        }
        
        vignette.intensity.value = minVignetteIntensity;
    }
    
    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.5f);
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.5f);
        }
    }
    
    private void OnGUI()
    {
        if (showDebugInfo && isActive)
        {
            GUI.Label(new Rect(10, 100, 300, 20), $"Vignette Progress: {currentProgress:F2}");
            GUI.Label(new Rect(10, 120, 300, 20), $"Vignette Intensity: {vignette?.intensity.value:F2}");
        }
    }
}

// Helper class for trigger
public class VignetteTriggerZone : MonoBehaviour
{
    public DistanceVignetteController controller;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.Activate();
            Debug.Log("Vignette distance tracking activated!");
        }
    }
}