using UnityEngine;
using System.Collections;

public class SinkInteractable : InteractableObject
{
    [SerializeField] private string sinkCutsceneID = "sink_cutscene";
    
    public override void OnInteract()
    {
        base.OnInteract();
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutscene(sinkCutsceneID);
        }
        else
        {
            Debug.LogError("CutsceneManager instance is null!");
        }
    }
}