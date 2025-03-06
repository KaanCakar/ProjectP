using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour, Interactable
{
    [SerializeField] private float interactionDistance = 3f;
    
    public float InteractionDistance => interactionDistance;

    public virtual void OnInteract()
    {
    }

    public virtual void OnFocus()
    {
    }

    public virtual void OnLoseFocus()
    {
    }
}