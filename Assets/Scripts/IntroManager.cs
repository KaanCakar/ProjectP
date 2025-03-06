using UnityEngine;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private string introCutsceneID = "intro_cutscene";
    
    private void Start()
    {
        CutsceneManager.Instance.PlayCutscene(introCutsceneID);
    }
}