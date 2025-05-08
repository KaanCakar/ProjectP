using UnityEngine;

public class SirenLight : MonoBehaviour
{
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
}