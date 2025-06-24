using UnityEngine;

public class SirenLight : MonoBehaviour
{

    /// <summary>
    /// Kaan ÇAKAR - 2025
    /// SirenLight is a Unity component that simulates a rotating siren light with flickering effects.
    /// </summary>


    [Header("Rotation Settings")]
    [Range(0f, 1000f)]
    public float rotationSpeed = 360f;
    public Vector3 rotationAxis = Vector3.right;

    [Header("Light Settings")]
    public Light spotLight;
    public bool enableFlicker = false;
    [Range(0.01f, 1f)]
    public float flickerFrequency = 0.1f;
    [Range(0f, 1f)]
    public float flickerIntensity = 0.2f;

    [Header("Emergency Effect")]
    public bool emergencyMode = false;
    [Range(1f, 5f)]
    public float emergencySpeedMultiplier = 2f;
    [Range(1f, 3f)]
    public float emergencyIntensityMultiplier = 1.5f;

    [Header("Color Control")]
    public bool enableColorTransition = false;
    public Color normalColor = Color.white;
    public Color emergencyColor = Color.red;
    public float colorTransitionSpeed = 2f;
    private Color currentTargetColor;
    private Color originalColor;

    private float baseIntensity;
    private float timeSinceLastFlicker = 0f;
    private Quaternion initialRotation;

    private void Start()
    {
        if (spotLight == null)
        {
            spotLight = GetComponent<Light>();
            if (spotLight == null)
            {
                Debug.LogError("No Light component found on " + gameObject.name + ". Please add a Light component or assign one in the inspector.");
                enabled = false;
                return;
            }
        }

        baseIntensity = spotLight.intensity;
        initialRotation = transform.localRotation;
        originalColor = spotLight.color;
        currentTargetColor = normalColor;
    }

    private void Update()
    {
        float currentRotationSpeed = rotationSpeed;
        float currentIntensity = baseIntensity;

        if (emergencyMode)
        {
            currentRotationSpeed *= emergencySpeedMultiplier;
            currentIntensity *= emergencyIntensityMultiplier;
        }

        transform.Rotate(rotationAxis, currentRotationSpeed * Time.deltaTime);

        if (enableColorTransition)
        {
            currentTargetColor = emergencyMode ? emergencyColor : normalColor;
            spotLight.color = Color.Lerp(spotLight.color, currentTargetColor, Time.deltaTime * colorTransitionSpeed);
        }

        if (enableFlicker)
        {
            timeSinceLastFlicker += Time.deltaTime;

            if (timeSinceLastFlicker >= flickerFrequency)
            {
                float randomIntensity = Random.Range(currentIntensity * (1f - flickerIntensity), currentIntensity);
                spotLight.intensity = randomIntensity;
                timeSinceLastFlicker = 0f;
            }
        }
        else
        {
            spotLight.intensity = currentIntensity;
        }
    }

    public void SetEmergencyMode(bool active)
    {
        emergencyMode = active;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public void EnableLight(bool enabled)
    {
        spotLight.enabled = enabled;
    }

    public void ResetRotation()
    {
        transform.localRotation = initialRotation;
    }

    public void SetColorTransition(bool enable)
    {
        enableColorTransition = enable;
    }

    public void TriggerDangerMode()
    {
        SetEmergencyMode(true);
        SetColorTransition(true);
    }

    public void ResetToNormal()
    {
        SetEmergencyMode(false);
        SetColorTransition(true);
    }
}