using UnityEngine;

public class BloodInteractable : InteractableObject
{
    [SerializeField] private string bloodCutsceneID = "blood_cutscene";
    
    public override void OnInteract()
    {
        base.OnInteract();
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutscene(bloodCutsceneID);
        }
        else
        {
            Debug.LogError("CutsceneManager instance is null!");
        }
    }
}