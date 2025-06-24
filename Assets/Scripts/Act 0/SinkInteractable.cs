using UnityEngine;
using System.Collections;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// SinkInteractable is a component that triggers a cutscene when the player interacts with a sink
/// </summary>
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