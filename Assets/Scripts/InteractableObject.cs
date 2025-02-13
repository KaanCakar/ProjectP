using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour, Interactable
{
    [SerializeField] private float interactionDistance = 3f;
    
    public float InteractionDistance => interactionDistance;

    public void OnInteract()
    {
        Debug.Log($"Interacted with {gameObject.name}");
    }

    public void OnFocus()
    {
        Debug.Log($"Focusing on {gameObject.name}");
    }

    public void OnLoseFocus()
    {
        Debug.Log($"Lost focus from {gameObject.name}");
    }
}