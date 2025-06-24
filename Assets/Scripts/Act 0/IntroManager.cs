using UnityEngine;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// IntroManager is responsible for playing the introductory cutscene when the game starts.
/// </summary>
public class IntroManager : MonoBehaviour
{
    [SerializeField] private string introCutsceneID = "intro_cutscene";

    private void Start()
    {
        CutsceneManager.Instance.PlayCutscene(introCutsceneID);
    }
}