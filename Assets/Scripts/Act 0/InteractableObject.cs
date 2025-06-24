using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// InteractableObject is a base class for objects that can be interacted with in the game.
/// </summary>
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