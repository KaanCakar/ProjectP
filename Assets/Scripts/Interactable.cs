public interface Interactable
{
    float InteractionDistance { get; }
    void OnInteract();
    void OnFocus();
    void OnLoseFocus();
}