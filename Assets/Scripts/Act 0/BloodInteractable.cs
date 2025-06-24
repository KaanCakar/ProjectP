using UnityEngine;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// BloodInteractable triggers a cutscene when the player interacts with it.
/// </summary>
public class BloodInteractable : InteractableObject
{
    [SerializeField] private string bloodCutsceneID = "blood_cutscene";
    [SerializeField] private string nextCutsceneID = "awakening_cutscene";

    public override void OnInteract()
    {
        base.OnInteract();
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutsceneSequence(bloodCutsceneID, nextCutsceneID);
        }
        else
        {
            Debug.LogError("CutsceneManager instance is null!");
        }
    }
}