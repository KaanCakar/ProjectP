using UnityEngine;
using System.Collections;

public class SinkInteractable : InteractableObject
{
    [Header("Interaction Position")]
    [SerializeField] private Vector3 interactionPosition = new Vector3(-4.27111f, 0.93f, 7.48280f);
    [SerializeField] private Vector3 interactionRotation = new Vector3(0f, -1.05f, 0f);
    [SerializeField] private Vector3 targetCameraRotation = new Vector3(53.7f, 0f, 0f);
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.1f;
    [SerializeField] private float rotationSpeed = 2f;

    private FPSController playerController;
    private CharacterController characterController;
    private Camera playerCamera;
    private bool isMovingToPosition = false;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastCameraRotation; 
    private Rigidbody playerRigidbody;

    private void Awake()
    {
        playerController = FindObjectOfType<FPSController>();
        characterController = playerController.GetComponent<CharacterController>();
        playerCamera = playerController.GetComponentInChildren<Camera>();
        playerRigidbody = playerController.GetComponent<Rigidbody>();
    }

    public override void OnInteract()
    {
        if (!isMovingToPosition)
        {
            lastPosition = playerController.transform.position;
            lastRotation = playerController.transform.rotation;
            lastCameraRotation = playerCamera.transform.localRotation.eulerAngles;
            StartCoroutine(MovePlayerToPosition());
        }
    }

    private IEnumerator MovePlayerToPosition()
    {
        isMovingToPosition = true;
        playerController.enabled = false;

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
        }

        Transform playerTransform = playerController.transform;
        bool hasReachedPosition = false;

        while (!hasReachedPosition)
        {
            float distanceToTarget = Vector3.Distance(playerTransform.position, interactionPosition);

            if (distanceToTarget <= stopDistance)
            {
                hasReachedPosition = true;
                break;
            }

            Vector3 moveDirection = (interactionPosition - playerTransform.position).normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            
            characterController.Move(movement);

            Quaternion targetRotation = Quaternion.Euler(0f, interactionRotation.y, 0f);
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            Vector3 currentCameraRotation = playerCamera.transform.localRotation.eulerAngles;
            Vector3 newRotation = new Vector3(
                Mathf.LerpAngle(currentCameraRotation.x, targetCameraRotation.x, Time.deltaTime * rotationSpeed),
                Mathf.LerpAngle(currentCameraRotation.y, targetCameraRotation.y, Time.deltaTime * rotationSpeed),
                Mathf.LerpAngle(currentCameraRotation.z, targetCameraRotation.z, Time.deltaTime * rotationSpeed)
            );
            playerCamera.transform.localRotation = Quaternion.Euler(newRotation);

            yield return null;
        }

        // Pozisyona ulaşıldığında
        characterController.enabled = false;
        yield return new WaitForSeconds(0.1f);

        // Kesin pozisyonlara yerleştir
        playerTransform.position = interactionPosition;
        playerTransform.rotation = Quaternion.Euler(0f, interactionRotation.y, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(targetCameraRotation);

        // Karakteri kilitle
        StartCoroutine(LockPosition());
        
        isMovingToPosition = false;
        Debug.Log("Position locked!");
    }

    private IEnumerator LockPosition()
    {
        while (true)
        {
            playerController.transform.position = interactionPosition;
            playerController.transform.rotation = Quaternion.Euler(0f, interactionRotation.y, 0f);
            playerCamera.transform.localRotation = Quaternion.Euler(targetCameraRotation);
            
            if (characterController.enabled)
            {
                characterController.enabled = false;
            }
            
            yield return null;
        }
    }

    public void ResetPlayerPosition()
    {
        StopAllCoroutines();
        characterController.enabled = false;
        
        playerController.transform.position = lastPosition;
        playerController.transform.rotation = lastRotation;
        playerCamera.transform.localRotation = Quaternion.Euler(lastCameraRotation);
        
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        
        characterController.enabled = true;
        playerController.enabled = true;
        isMovingToPosition = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionPosition, 0.3f);
        
        Vector3 direction = Quaternion.Euler(interactionRotation) * Vector3.forward;
        Gizmos.DrawRay(interactionPosition, direction);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(interactionPosition, stopDistance);
    }
}