using UnityEngine;
using System.Collections;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// DoorInteraction handles the interaction with doors, allowing them to be opened and closed with animations.
/// <summary>
public class DoorInteraction : InteractableObject
{
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";
    [SerializeField] private string closeAnimationTrigger = "Close";
    [SerializeField] private AudioSource doorSound;
    [SerializeField] private float animationDuration = 4f;

    private bool isOpen = false;
    private Collider doorCollider;

    private void Awake()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }

        if (doorSound == null && TryGetComponent(out AudioSource audioSource))
        {
            doorSound = audioSource;
        }

        doorCollider = GetComponent<Collider>();
    }

    public override void OnInteract()
    {
        StartCoroutine(TemporarilyDisableCollision());

        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }

        isOpen = !isOpen;
    }

    private IEnumerator TemporarilyDisableCollision()
    {
        if (doorCollider != null)
        {
            bool originalTriggerState = doorCollider.isTrigger;
            doorCollider.isTrigger = true;

            yield return new WaitForSeconds(animationDuration);
            doorCollider.isTrigger = originalTriggerState;
        }
    }

    private void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }

        if (doorSound != null)
        {
            doorSound.Play();
        }
    }

    private void CloseDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(closeAnimationTrigger);
        }

        if (doorSound != null)
        {
            doorSound.Play();
        }
    }

    public override void OnFocus()
    {
        // Kapıya yakınken fısıltı sesleri?
    }

    public override void OnLoseFocus()
    {
        // kapıdan uzakken sesleri susturma?
    }
}