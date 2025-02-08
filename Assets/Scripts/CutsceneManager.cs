using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public PlayableDirector timelineDirector;
    public FPSController playerController;
    public Animator characterAnimator;
    
    private void Start()
    {
        if (!PlayerPrefs.HasKey("CutscenePlayed"))
        {
            if (playerController != null)
            {
                playerController.enabled = false;
                Cursor.lockState = CursorLockMode.None;
            }

            if (timelineDirector != null)
            {
                timelineDirector.played += OnPlayableDirectorStarted;
                timelineDirector.stopped += OnPlayableDirectorStopped;
                timelineDirector.Play();
            }

            PlayerPrefs.SetInt("CutscenePlayed", 1);
            PlayerPrefs.Save();
        }
        else
        {
            if (timelineDirector != null)
            {
                timelineDirector.gameObject.SetActive(false);
            }

            if (playerController != null)
            {
                playerController.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
            }
            if (characterAnimator != null)
            {
                characterAnimator.enabled = false;
                
            }
        }
    }

    private void OnPlayableDirectorStarted(PlayableDirector director)
    {
        Debug.Log("Cutscene started");
    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (playerController != null)
        {
            playerController.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (characterAnimator != null)
        {
            characterAnimator.enabled = false;

        }

        timelineDirector.played -= OnPlayableDirectorStarted;
        timelineDirector.stopped -= OnPlayableDirectorStopped;
    }
}