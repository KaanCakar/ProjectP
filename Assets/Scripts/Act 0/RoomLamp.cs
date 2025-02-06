using UnityEngine;
using UnityEngine.Rendering;
public class RoomLamp : MonoBehaviour
{
    [Header("Light Settings")]
    public Light mainLight;
    public LensFlareComponentSRP lensFlare;

    [Header("Flicker Interval")]
    public float minFlickerInterval = 5f;
    public float maxFlickerInterval = 8f;
    public int minFlickerCount = 2;
    public int maxFlickerCount = 3;

    [Header("Light Intensity Control")]
    public float maxNormalIntensity = 1f;
    public float minFlickerIntensity = 0.3f;
    public float maxFlickerIntensity = 0.7f;

    [Header("Audio Settings")]
    public AudioSource flickerSoundSource;
    public AudioClip[] flickerSounds;

    private float nextFlickerTime;
    private int remainingFlickers;
    private float flickerDuration = 0.1f;
    private float currentFlickerTimer;
    private bool isFlickering = false;
    private float originalIntensity;

    void Start()
    {
        originalIntensity = mainLight.intensity;
        SetNextFlickerTime();
    }

    void Update()
    {
        if (Time.time >= nextFlickerTime && !isFlickering)
        {
            StartFlickerSequence();
        }

        if (isFlickering)
        {
            HandleFlickerSequence();
        }
    }

    void SetNextFlickerTime()
    {
        nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
        remainingFlickers = Random.Range(minFlickerCount, maxFlickerCount + 1);
        isFlickering = false;
        mainLight.intensity = originalIntensity;
    }

    void StartFlickerSequence()
    {
        isFlickering = true;
        currentFlickerTimer = 0;
        remainingFlickers = Random.Range(minFlickerCount, maxFlickerCount + 1);
    }

    void HandleFlickerSequence()
    {
        currentFlickerTimer += Time.deltaTime;

        if (currentFlickerTimer < flickerDuration)
        {
            float flickerIntensity = Random.Range(minFlickerIntensity, maxFlickerIntensity);
            mainLight.intensity = flickerIntensity;
            
            if (lensFlare != null)
            {
                lensFlare.intensity = Random.Range(0.5f, 1.5f);
            }
            if (flickerSoundSource != null && flickerSounds.Length > 0)
            {
                flickerSoundSource.PlayOneShot(flickerSounds[Random.Range(0, flickerSounds.Length)]);
            }
        }
        else
        {
            mainLight.intensity = originalIntensity;
            
            if (lensFlare != null)
            {
                lensFlare.intensity = 1f;
            }

            remainingFlickers--;

            if (remainingFlickers > 0)
            {
                currentFlickerTimer = 0;
            }
            else
            {
                SetNextFlickerTime();
            }
        }
    }
}