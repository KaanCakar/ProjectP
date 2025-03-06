using UnityEngine;

public class DoorInteraction : InteractableObject
{
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";
    [SerializeField] private string closeAnimationTrigger = "Close";
    [SerializeField] private AudioSource doorSound;
    
    private bool isOpen = false;

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
    }

    public override void OnInteract()
    {
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