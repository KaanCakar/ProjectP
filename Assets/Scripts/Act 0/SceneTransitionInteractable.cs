using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script handles scene transitions when the player interacts with a specific object.
/// </summary>
public class SceneTransitionInteractable : InteractableObject
{
    [Header("Scene Transition Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private float transitionDelay = 1f;
    [SerializeField] private bool useFade = true;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Pre-Transition Events")]
    [SerializeField] private UnityEvent onTransitionStart;
    [SerializeField] private UnityEvent onTransitionComplete;

    [Header("Save Progress")]
    [SerializeField] private bool saveProgress = true;
    [SerializeField] private string checkpointName = "Act1_Start";

    private bool isTransitioning = false;

    public override void OnInteract()
    {
        if (isTransitioning) return;

        base.OnInteract();
        StartCoroutine(TransitionSequence());
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // Trigger pre-transition events
        onTransitionStart?.Invoke();

        // Disable player controls
        FPSController player = FindObjectOfType<FPSController>();
        if (player != null)
        {
            player.enabled = false;
        }

        // Start fade if enabled
        if (useFade && fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        else
        {
            yield return new WaitForSeconds(transitionDelay);
        }

        // Save progress if enabled
        if (saveProgress)
        {
            SaveProgress();
        }

        // Trigger completion events
        onTransitionComplete?.Invoke();

        // Load the scene
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("Target scene name is not set!");
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetString("LastCheckpoint", checkpointName);
        PlayerPrefs.SetString("LastScene", targetSceneName);
        PlayerPrefs.Save();

        Debug.Log($"Progress saved: {checkpointName}");
    }

    public override void OnFocus()
    {
    }

    public override void OnLoseFocus()
    {
    }
}