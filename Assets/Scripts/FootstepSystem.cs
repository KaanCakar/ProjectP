using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script manages the footstep sounds and surface detection for a first-person character.
/// </summary>

[System.Serializable]
public class FootstepSurface
{
    public string surfaceTag;
    public string surfaceMaterialName; 
    public string surfaceLayer;        
    public AudioClip[] footstepSounds;
    public AudioClip[] runSounds; 
    [Range(0f, 2f)]
    public float volumeMultiplier = 1f;
    [Range(0.8f, 1.2f)]
    public float pitchRange = 1.05f; 
}

public class FootstepSystem : MonoBehaviour
{
    [Header("Footstep Settings")]
    public FootstepSurface[] surfaces;
    
    public AudioClip[] defaultFootsteps;
    
    [Header("Audio Settings")]
    public AudioSource footstepAudioSource;
    
    [Range(0f, 1f)]
    public float masterVolume = 0.5f;
    
    public float groundCheckDistance = 1.2f;
    
    [Header("Step Timing")]
    public float walkStepInterval = 0.5f;
    
    public float runStepInterval = 0.3f;
    
    public float runningThreshold = 6f;
    
    [Header("Debugging")]
    public bool showDebugInfo = false;
    
    private FPSController playerController;
    private float stepTimer = 0f;
    private float currentStepInterval;
    private Vector3 lastPosition;
    private bool isGrounded = true;
    private Dictionary<string, FootstepSurface> surfaceLookup = new Dictionary<string, FootstepSurface>();
    
    private FootstepSurface currentSurface = null;
    private string lastSurfaceDebugInfo = "None";
    
    void Start()
    {
        playerController = GetComponent<FPSController>();
        
        if (footstepAudioSource == null)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.playOnAwake = false;
            footstepAudioSource.spatialBlend = 1.0f;
            footstepAudioSource.priority = 128;
            footstepAudioSource.volume = masterVolume;
        }
        
        BuildSurfaceLookup();
        
        lastPosition = transform.position;
    }
    
    void BuildSurfaceLookup()
    {
        foreach (FootstepSurface surface in surfaces)
        {
            if (!string.IsNullOrEmpty(surface.surfaceTag))
                surfaceLookup[surface.surfaceTag.ToLower()] = surface;
                
            if (!string.IsNullOrEmpty(surface.surfaceMaterialName))
                surfaceLookup[surface.surfaceMaterialName.ToLower()] = surface;
                

            if (!string.IsNullOrEmpty(surface.surfaceLayer))
                surfaceLookup[surface.surfaceLayer.ToLower()] = surface;
        }
    }
    
    void Update()
    {
        if (playerController == null) return;
        
        Vector3 horizontalMovement = new Vector3(transform.position.x - lastPosition.x, 0, transform.position.z - lastPosition.z);
        float movementMagnitude = horizontalMovement.magnitude;
        lastPosition = transform.position;
        
        bool isRunning = playerController.currentSpeed > runningThreshold;
        currentStepInterval = isRunning ? runStepInterval : walkStepInterval;

        if (movementMagnitude > 0.01f && isGrounded)
        {
            stepTimer += Time.deltaTime;
            
            float speedRatio = playerController.currentSpeed / playerController.sprintSpeed;
            float adjustedInterval = Mathf.Lerp(walkStepInterval, runStepInterval, speedRatio);
            
            if (stepTimer >= adjustedInterval)
            {
                stepTimer = 0f;
                PlayFootstepSound(isRunning);
            }
        }
        else
        {
            stepTimer = 0f;
        }
        
        CheckGroundSurface();
    }
    
    void CheckGroundSurface()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);
        
        if (Physics.Raycast(ray, out hit, groundCheckDistance))
        {
            isGrounded = true;
            
            string surfaceTag = hit.collider.tag.ToLower();
            if (surfaceLookup.ContainsKey(surfaceTag))
            {
                currentSurface = surfaceLookup[surfaceTag];
                lastSurfaceDebugInfo = $"Tag: {surfaceTag}";
                return;
            }
            
            if (hit.collider.GetComponent<Renderer>() != null)
            {
                string materialName = hit.collider.GetComponent<Renderer>().material.name.ToLower();
                materialName = materialName.Replace(" (instance)", "");
                
                if (surfaceLookup.ContainsKey(materialName))
                {
                    currentSurface = surfaceLookup[materialName];
                    lastSurfaceDebugInfo = $"Material: {materialName}";
                    return;
                }
            }
            
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer).ToLower();
            if (surfaceLookup.ContainsKey(layerName))
            {
                currentSurface = surfaceLookup[layerName];
                lastSurfaceDebugInfo = $"Layer: {layerName}";
                return;
            }
            
            currentSurface = null;
            lastSurfaceDebugInfo = "Default";
        }
        else
        {
            isGrounded = false;
            currentSurface = null;
            lastSurfaceDebugInfo = "Not Grounded";
        }
    }
    
    void PlayFootstepSound(bool isRunning)
    {
        if (!footstepAudioSource) return;
        
        AudioClip[] soundArray;
        
        if (currentSurface != null)
        {
            if (isRunning && currentSurface.runSounds != null && currentSurface.runSounds.Length > 0)
                soundArray = currentSurface.runSounds;
            else
                soundArray = currentSurface.footstepSounds;
                
            footstepAudioSource.volume = masterVolume * currentSurface.volumeMultiplier;
        }
        else
        {
            soundArray = defaultFootsteps;
            footstepAudioSource.volume = masterVolume;
        }
        
        if (soundArray == null || soundArray.Length == 0) return;
        
        AudioClip randomClip = soundArray[Random.Range(0, soundArray.Length)];
        if (randomClip == null) return;
        
        float randomPitch = 1.0f;
        if (currentSurface != null)
            randomPitch = Random.Range(1.0f / currentSurface.pitchRange, currentSurface.pitchRange);
        
        footstepAudioSource.pitch = randomPitch;
        
        footstepAudioSource.PlayOneShot(randomClip);
    }
    
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Surface: {lastSurfaceDebugInfo}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Step Timer: {stepTimer:F2} / {currentStepInterval:F2}");
            GUI.Label(new Rect(10, 50, 300, 20), $"Speed: {playerController.currentSpeed:F2}");
            GUI.Label(new Rect(10, 70, 300, 20), $"Grounded: {isGrounded}");
        }
    }
}