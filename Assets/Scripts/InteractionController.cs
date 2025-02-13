using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Image crosshair;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color interactableColor = Color.green;
    
    [SerializeField] private bool showDebugRay = true;

    private Camera playerCamera;
    private Interactable currentInteractable;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (crosshair == null)
        {
            Debug.LogError("Crosshair image is not assigned to InteractionController!");
            enabled = false;
            return;
        }

        crosshair.color = normalColor;
        
        Debug.Log($"InteractionController initialized. Layer mask: {interactableLayer.value}");
    }

    private void Update()
    {
        HandleInteractionRay();
        HandleInteractionInput();
    }

    private void HandleInteractionRay()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
        }

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name} on layer: {hit.collider.gameObject.layer}");
            
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null && hit.distance <= interactable.InteractionDistance)
            {
                Debug.Log($"Valid interactable found: {hit.collider.gameObject.name}");
                
                if (currentInteractable != interactable)
                {
                    currentInteractable?.OnLoseFocus();
                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
                    UpdateCrosshair(true);
                }
            }
            else
            {
                Debug.Log($"No valid interactable component or out of range. Distance: {hit.distance}");
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
            UpdateCrosshair(false);
        }
    }

    private void UpdateCrosshair(bool isInteractable)
    {
        if (crosshair != null)
        {
            Debug.Log($"Updating crosshair color to: {(isInteractable ? "Interactable" : "Normal")}");
            crosshair.color = isInteractable ? interactableColor : normalColor;
        }
    }
}