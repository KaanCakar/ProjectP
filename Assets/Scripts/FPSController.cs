using UnityEngine;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script controls the first-person shooter character's movement, camera look, and various gameplay mechanics
/// </summary>

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float accelerationTime = 0.5f;
    public float decelerationTime = 0.3f;
    public float mouseSensitivity = 2f;
    public float playerHeight = 1.8f;
    public float cameraHeightRatio = 0.9f;

    [Header("Sprint Visual Effects")]
    public float sprintFOVIncrease = 10f;
    public float fovTransitionSpeed = 5f;
    public float minSpeedForFOV = 5.5f;

    [Header("Breathing")]
    public float breathingSpeed = 1f;
    public float breathingAmount = 0.05f;
    public float idleBreathMultiplier = 2f;
    public float sprintBreathMultiplier = 0.5f;
    public float idleTimeRequired = 3f;
    public float breathTransitionSpeed = 5f;

    [Header("Walking & Running")]
    public float walkBobSpeed = 10f;
    public float sprintBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float sprintBobAmount = 0.08f;
    public float verticalBobAmount = 0.05f;
    public float sprintVerticalBobAmount = 0.08f;
    public float horizontalBobAmount = 0.05f;
    public float sprintHorizontalBobAmount = 0.08f;
    public float bobTransitionSpeed = 8f;

    [Header("Hallucination Effects")]
    public bool useHallucinationSystem = true;
    public float hallucinatedLookMultiplier = 0.5f;

    [HideInInspector] public bool invertMouseX = false;
    [HideInInspector] public bool invertMouseY = false;
    [HideInInspector] public bool swapMovementKeys = false;

    // Make currentSpeed public so FootstepSystem can access it
    [HideInInspector] public float currentSpeed;

    private CharacterController characterController;
    private Camera playerCamera;
    private float verticalRotation = 0f;
    private float idleTimer = 0f;
    private float defaultCameraY;
    private float bobTimer = 0f;
    private float currentBobMultiplier = 0f;
    private float currentBreathMultiplier = 1f;
    private float lastBreathValue = 0f;
    private Vector3 lastBobPosition = Vector3.zero;
    private float defaultFOV;
    private float targetFOV;
    private float targetSpeed;
    private float velocityChangeRate;
    private Vector3 currentVelocity;
    private bool isMoving;
    private HallucinationSystem hallucinationSystem;
    private FootstepSystem footstepSystem;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        hallucinationSystem = GetComponent<HallucinationSystem>();
        footstepSystem = GetComponent<FootstepSystem>();

        Cursor.lockState = CursorLockMode.Locked;
        defaultFOV = playerCamera.fieldOfView;
        targetFOV = defaultFOV;
        currentSpeed = walkSpeed;
        targetSpeed = walkSpeed;

        characterController.height = playerHeight;
        characterController.center = new Vector3(0, playerHeight / 2f, 0);
        defaultCameraY = playerHeight * cameraHeightRatio;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        UpdateFOV();
        Vector3 finalPosition = CalculateCameraPosition();
        playerCamera.transform.localPosition = finalPosition;
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (swapMovementKeys)
        {
            moveX = -moveX;
            moveZ = -moveZ;
        }

        if (useHallucinationSystem && hallucinationSystem != null)
        {
            Vector2 hallucinationOffset = hallucinationSystem.GetHallucinationMovementOffset();
            moveX += hallucinationOffset.x;
            moveZ += hallucinationOffset.y;

            moveX = Mathf.Clamp(moveX, -1f, 1f);
            moveZ = Mathf.Clamp(moveZ, -1f, 1f);
        }

        isMoving = (moveX != 0 || moveZ != 0);

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving;
        targetSpeed = wantsToSprint ? sprintSpeed : walkSpeed;

        float accelerationRate = wantsToSprint ? accelerationTime : decelerationTime;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / accelerationRate);

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        if (moveDirection.magnitude > 0)
        {
            moveDirection = moveDirection.normalized;
        }

        Vector3 targetVelocity = moveDirection * currentSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime / accelerationRate);
        characterController.Move(currentVelocity * Time.deltaTime);

        float targetBobMultiplier = isMoving ? 1f : 0f;
        currentBobMultiplier = Mathf.Lerp(currentBobMultiplier, targetBobMultiplier, Time.deltaTime * bobTransitionSpeed);

        float speedRatio = Mathf.InverseLerp(walkSpeed, sprintSpeed, currentSpeed);
        bool shouldIncreaseFOV = currentSpeed > minSpeedForFOV && isMoving;
        targetFOV = shouldIncreaseFOV ?
            Mathf.Lerp(defaultFOV, defaultFOV + sprintFOVIncrease, speedRatio) :
            defaultFOV;
    }

    void UpdateFOV()
    {
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (invertMouseX) mouseX = -mouseX;
        if (invertMouseY) mouseY = -mouseY;

        if (useHallucinationSystem && hallucinationSystem != null)
        {
            Vector2 hallucinationOffset = hallucinationSystem.GetHallucinationMovementOffset();
            mouseX += hallucinationOffset.x * hallucinatedLookMultiplier;
            mouseY += hallucinationOffset.y * hallucinatedLookMultiplier;
        }

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    Vector3 CalculateCameraPosition()
    {
        Vector3 pos = new Vector3(0, defaultCameraY, 0);

        float speedRatio = Mathf.InverseLerp(walkSpeed, sprintSpeed, currentSpeed);
        float breathMultiplier = Mathf.Lerp(1f, sprintBreathMultiplier, speedRatio);

        float breathing = CalculateBreathing() * breathMultiplier;
        pos.y += breathing * currentBreathMultiplier;

        Vector3 bobEffect = CalculateBobEffect(speedRatio);
        pos += Vector3.Lerp(lastBobPosition, bobEffect * currentBobMultiplier, Time.deltaTime * bobTransitionSpeed);
        lastBobPosition = bobEffect * currentBobMultiplier;

        return pos;
    }

    float CalculateBreathing()
    {
        if (!isMoving)
        {
            idleTimer += Time.deltaTime;
        }
        else
        {
            idleTimer = 0f;
        }

        float targetBreathMultiplier = 1f;
        if (idleTimer > idleTimeRequired)
        {
            targetBreathMultiplier = idleBreathMultiplier;
        }
        currentBreathMultiplier = Mathf.Lerp(currentBreathMultiplier, targetBreathMultiplier, Time.deltaTime * breathTransitionSpeed);

        float newBreathValue = Mathf.Sin(Time.time * breathingSpeed) * breathingAmount;
        float smoothBreathing = Mathf.Lerp(lastBreathValue, newBreathValue, Time.deltaTime * breathTransitionSpeed);
        lastBreathValue = smoothBreathing;

        return smoothBreathing;
    }

    Vector3 CalculateBobEffect(float speedRatio)
    {
        if (currentBobMultiplier > 0.01f)
        {
            float currentBobSpeed = Mathf.Lerp(walkBobSpeed, sprintBobSpeed, speedRatio);
            bobTimer += Time.deltaTime * currentBobSpeed;
        }

        Vector3 bobPos = Vector3.zero;
        float vertAmount = Mathf.Lerp(verticalBobAmount, sprintVerticalBobAmount, speedRatio);
        float horizAmount = Mathf.Lerp(horizontalBobAmount, sprintHorizontalBobAmount, speedRatio);

        bobPos.y = Mathf.Sin(bobTimer) * vertAmount;
        bobPos.x = Mathf.Cos(bobTimer * 0.5f) * horizAmount;

        return bobPos;
    }

    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsRunning()
    {
        return currentSpeed > (walkSpeed + 0.5f);
    }
}