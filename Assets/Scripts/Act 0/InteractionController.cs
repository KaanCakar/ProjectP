using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// InteractionController manages player interactions with interactable objects in the game world.
/// </summary>
public class InteractionController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Image crosshair;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color interactableColor = Color.green;

    private Camera playerCamera;
    private Interactable currentInteractable;
    private static readonly Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0f);

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (crosshair != null)
        {
            crosshair.color = normalColor;
        }
    }

    private void Update()
    {
        HandleInteractionRay();
        HandleInteractionInput();
    }

    private void HandleInteractionRay()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(screenCenter), out RaycastHit hit, rayDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null && hit.distance <= interactable.InteractionDistance)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable?.OnLoseFocus();
                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
                    if (crosshair != null) crosshair.color = interactableColor;
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    private void HandleInteractionInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.OnInteract();
        }
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
            if (crosshair != null) crosshair.color = normalColor;
        }
    }
}