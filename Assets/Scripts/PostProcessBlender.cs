using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// PostProcessBlender manages the blending of post-processing effects between normal and hallucination states.
/// </summary>
public class PostProcessBlender : MonoBehaviour
{
    [Header("Volume Profiles")]
    [SerializeField] private VolumeProfile normalProfile;
    [SerializeField] private VolumeProfile hallucinationProfile;
    [SerializeField] private Volume mainVolume;

    [Header("Blend Settings")]
    [SerializeField] private float blendSpeed = 2f;

    [Header("Override Settings")]
    [SerializeField] private bool manualOverrideControl = true;

    // Effect references
    private Vignette normalVignette;
    private Vignette hallucinationVignette;
    private ChromaticAberration normalChromatic;
    private ChromaticAberration hallucinationChromatic;
    private Bloom normalBloom;
    private Bloom hallucinationBloom;

    private float currentBlendWeight = 0f;
    private float targetBlendWeight = 0f;

    // Temporary override holders
    private VolumeProfile runtimeProfile;

    private void Start()
    {
        if (mainVolume == null)
        {
            mainVolume = GetComponent<Volume>();
        }

        // Create runtime profile
        CreateRuntimeProfile();

        // Cache effect references
        CacheEffectReferences();
    }

    private void CreateRuntimeProfile()
    {
        // Clone the normal profile as base
        runtimeProfile = Instantiate(normalProfile);
        mainVolume.profile = runtimeProfile;

        // Ensure all necessary overrides exist
        if (!runtimeProfile.TryGet(out Vignette _))
        {
            runtimeProfile.Add<Vignette>();
        }
        if (!runtimeProfile.TryGet(out ChromaticAberration _))
        {
            runtimeProfile.Add<ChromaticAberration>();
        }
        if (!runtimeProfile.TryGet(out Bloom _))
        {
            runtimeProfile.Add<Bloom>();
        }
    }

    private void CacheEffectReferences()
    {
        // Get references from profiles
        normalProfile.TryGet(out normalVignette);
        normalProfile.TryGet(out normalChromatic);
        normalProfile.TryGet(out normalBloom);

        hallucinationProfile.TryGet(out hallucinationVignette);
        hallucinationProfile.TryGet(out hallucinationChromatic);
        hallucinationProfile.TryGet(out hallucinationBloom);

        // Get runtime references
        runtimeProfile.TryGet(out Vignette runtimeVignette);
        runtimeProfile.TryGet(out ChromaticAberration runtimeChromatic);
        runtimeProfile.TryGet(out Bloom runtimeBloom);
    }

    private void Update()
    {
        // Smooth blend weight transition
        currentBlendWeight = Mathf.Lerp(currentBlendWeight, targetBlendWeight, Time.deltaTime * blendSpeed);

        // Update effects
        if (manualOverrideControl)
        {
            UpdateEffectBlends();
        }
    }

    private void UpdateEffectBlends()
    {
        Vignette runtimeVignette;
        ChromaticAberration runtimeChromatic;
        Bloom runtimeBloom;

        if (!runtimeProfile.TryGet(out runtimeVignette) ||
            !runtimeProfile.TryGet(out runtimeChromatic) ||
            !runtimeProfile.TryGet(out runtimeBloom))
        {
            return;
        }

        // Blend Vignette
        if (normalVignette != null && hallucinationVignette != null)
        {
            runtimeVignette.intensity.value = Mathf.Lerp(
                normalVignette.intensity.value,
                hallucinationVignette.intensity.value,
                currentBlendWeight
            );

            runtimeVignette.color.value = Color.Lerp(
                normalVignette.color.value,
                hallucinationVignette.color.value,
                currentBlendWeight
            );

            runtimeVignette.smoothness.value = Mathf.Lerp(
                normalVignette.smoothness.value,
                hallucinationVignette.smoothness.value,
                currentBlendWeight
            );
        }

        // Blend Chromatic Aberration
        if (normalChromatic != null && hallucinationChromatic != null)
        {
            runtimeChromatic.intensity.value = Mathf.Lerp(
                normalChromatic != null ? normalChromatic.intensity.value : 0f,
                hallucinationChromatic != null ? hallucinationChromatic.intensity.value : 1f,
                currentBlendWeight
            );
        }

        // Blend Bloom
        if (normalBloom != null && hallucinationBloom != null)
        {
            runtimeBloom.intensity.value = Mathf.Lerp(
                normalBloom.intensity.value,
                hallucinationBloom.intensity.value,
                currentBlendWeight
            );

            runtimeBloom.threshold.value = Mathf.Lerp(
                normalBloom.threshold.value,
                hallucinationBloom.threshold.value,
                currentBlendWeight
            );
        }
    }

    public void SetHallucinationLevel(float normalizedLevel)
    {
        targetBlendWeight = Mathf.Clamp01(normalizedLevel);
    }

    public void SetBlendSpeed(float speed)
    {
        blendSpeed = speed;
    }

    // Direct access for special cases
    public Vignette GetRuntimeVignette()
    {
        Vignette vignette;
        runtimeProfile.TryGet(out vignette);
        return vignette;
    }

    private void OnDestroy()
    {
        // Clean up runtime profile
        if (runtimeProfile != null)
        {
            Destroy(runtimeProfile);
        }
    }
}