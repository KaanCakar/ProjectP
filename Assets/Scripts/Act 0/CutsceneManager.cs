using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// CutsceneManager handles the playback of cutscenes using PlayableDirector.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    private static CutsceneManager instance;
    public static CutsceneManager Instance => instance;

    [System.Serializable]
    public class CutsceneData
    {
        public string cutsceneID;
        public PlayableDirector timelineDirector;
        public bool playOnce = true;
        public bool disablePlayerControl = true;
        public bool unlockCursor = false;
        [Header("Player Position")]
        public bool resetPlayerPosition = false;
        public Transform playerStartPosition;
        public bool fadeWhenResetting = true;
        public float fadeDuration = 0.5f;
        [Header("Camera Settings")]
        public bool useCutsceneCamera = false;
        public Camera cutsceneCamera;
        [Header("Player Object")]
        public bool disablePlayerObject = false;
        public bool enablePlayerAfterCutscene = false;
        [Header("Fade Settings")]
        public bool fadeAtStart = true;
        public bool fadeAtEnd = true;
        [Header("Objects")]
        public GameObject[] objectsToDisableAfterCutscene;
        [Header("Events")]
        public UnityEngine.Events.UnityEvent onCutsceneStart;
        public UnityEngine.Events.UnityEvent onCutsceneEnd;
    }

    public delegate void CutsceneEvent();
    public event CutsceneEvent OnCutsceneStarted;
    public event CutsceneEvent OnCutsceneEnded;

    [Header("Cutscene Settings")]
    [SerializeField] private CutsceneData[] cutscenes;

    [Header("References")]
    [SerializeField] private FPSController playerController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    private Coroutine fadeCoroutine;

    private Dictionary<string, CutsceneData> cutsceneDict = new Dictionary<string, CutsceneData>();
    private CutsceneData currentCutscene;
    private bool isCutscenePlaying = false;
    private bool wasPlayerControlEnabled = false;
    private CursorLockMode previousCursorState;
    private bool needsPositionResetBeforeNextCutscene = false;

    private string queuedCutsceneID = null;
    private bool queuedCutsceneIgnorePlayOnce = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerController == null)
            playerController = FindObjectOfType<FPSController>();

        if (playerCamera == null && playerController != null)
            playerCamera = playerController.GetComponentInChildren<Camera>();

        InitializeCutscenes();
    }

    private void InitializeCutscenes()
    {
        foreach (var cutscene in cutscenes)
        {
            cutsceneDict[cutscene.cutsceneID] = cutscene;

            if (cutscene.playOnce && HasCutscenePlayed(cutscene.cutsceneID))
            {
                DisableCutsceneObjects(cutscene);
            }

            if (cutscene.useCutsceneCamera && cutscene.cutsceneCamera != null)
            {
                cutscene.cutsceneCamera.gameObject.SetActive(false);
            }
        }
    }

    public void PlayCutscene(string cutsceneID, bool ignorePlayOnce = false)
    {
        if (isCutscenePlaying) return;

        if (cutsceneDict.TryGetValue(cutsceneID, out CutsceneData cutscene))
        {
            if (!ignorePlayOnce && cutscene.playOnce && HasCutscenePlayed(cutsceneID))
            {
                Debug.Log($"Cutscene {cutsceneID} has already been played");
                return;
            }

            currentCutscene = cutscene;
            StartCutscene();
        }
        else
        {
            Debug.LogWarning($"Cutscene with ID {cutsceneID} not found!");
        }
    }

    public void PlayCutsceneSequence(string firstCutsceneID, string nextCutsceneID)
    {
        queuedCutsceneID = nextCutsceneID;
        queuedCutsceneIgnorePlayOnce = true;

        if (cutsceneDict.TryGetValue(nextCutsceneID, out CutsceneData nextCutscene) &&
            nextCutscene.resetPlayerPosition)
        {
            needsPositionResetBeforeNextCutscene = true;
        }

        PlayCutscene(firstCutsceneID);
    }

    private void StartCutscene()
    {
        if (currentCutscene == null || currentCutscene.timelineDirector == null) return;

        isCutscenePlaying = true;

        wasPlayerControlEnabled = playerController != null && playerController.enabled;
        previousCursorState = Cursor.lockState;

        if (currentCutscene.disablePlayerObject && playerController != null)
        {
            playerController.gameObject.SetActive(false);
        }
        else if (currentCutscene.disablePlayerControl && playerController != null)
        {
            playerController.enabled = false;
        }

        if (currentCutscene.unlockCursor)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        if (currentCutscene.useCutsceneCamera && currentCutscene.cutsceneCamera != null)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            currentCutscene.cutsceneCamera.gameObject.SetActive(true);
        }

        if (currentCutscene.resetPlayerPosition &&
            currentCutscene.playerStartPosition != null &&
            playerController != null)
        {
            Transform playerTransform = playerController.transform;
            playerTransform.position = currentCutscene.playerStartPosition.position;
            playerTransform.rotation = currentCutscene.playerStartPosition.rotation;
        }

        currentCutscene.timelineDirector.played += OnPlayableDirectorStarted;
        currentCutscene.timelineDirector.stopped += OnPlayableDirectorStopped;

        currentCutscene.onCutsceneStart?.Invoke();

        OnCutsceneStarted?.Invoke();
        if (currentCutscene.fadeAtStart && fadeCanvasGroup != null)
        {
            StartCoroutine(FadeInAndPlayCutscene(currentCutscene.fadeDuration));
        }
        else
        {
            currentCutscene.timelineDirector.Play();
        }
    }

    private IEnumerator FadeInAndPlayCutscene(float duration)
    {
        fadeCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.2f);

        float startTime = Time.time;
        float endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            fadeCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        currentCutscene.timelineDirector.Play();
    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (currentCutscene == null) return;
        if (currentCutscene.fadeAtEnd && fadeCanvasGroup != null)
        {
            StartCoroutine(FadeOutAndFinishCutscene());
            return;
        }

        CompleteCutscene();
    }

    private IEnumerator FadeOutAndFinishCutscene()
    {
        float startTime = Time.time;
        float endTime = startTime + currentCutscene.fadeDuration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / currentCutscene.fadeDuration;
            fadeCanvasGroup.alpha = t;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        CompleteCutscene();
    }

    private void CompleteCutscene()
    {
        if (currentCutscene.useCutsceneCamera && currentCutscene.cutsceneCamera != null)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);

            currentCutscene.cutsceneCamera.gameObject.SetActive(false);
        }

        currentCutscene.onCutsceneEnd?.Invoke();

        OnCutsceneEnded?.Invoke();

        if (currentCutscene.playOnce)
        {
            SetCutscenePlayed(currentCutscene.cutsceneID);
            DisableCutsceneObjects(currentCutscene);
        }

        currentCutscene.timelineDirector.played -= OnPlayableDirectorStarted;
        currentCutscene.timelineDirector.stopped -= OnPlayableDirectorStopped;

        isCutscenePlaying = false;

        if (!string.IsNullOrEmpty(queuedCutsceneID) && needsPositionResetBeforeNextCutscene)
        {
            string nextCutsceneID = queuedCutsceneID;
            bool ignorePlayOnce = queuedCutsceneIgnorePlayOnce;

            queuedCutsceneID = null;
            queuedCutsceneIgnorePlayOnce = false;
            needsPositionResetBeforeNextCutscene = false;

            ResetPositionAndPlayNextCutscene(nextCutsceneID, ignorePlayOnce);
        }
        else if (!string.IsNullOrEmpty(queuedCutsceneID))
        {
            string nextCutsceneID = queuedCutsceneID;
            bool ignorePlayOnce = queuedCutsceneIgnorePlayOnce;

            queuedCutsceneID = null;
            queuedCutsceneIgnorePlayOnce = false;

            PlayCutscene(nextCutsceneID, ignorePlayOnce);
        }
        else
        {
            if (currentCutscene.enablePlayerAfterCutscene && playerController != null)
            {
                playerController.gameObject.SetActive(true);
                playerController.enabled = wasPlayerControlEnabled;
            }
            else if (currentCutscene.disablePlayerControl && playerController != null)
            {
                playerController.enabled = wasPlayerControlEnabled;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (currentCutscene.fadeAtEnd && fadeCanvasGroup != null)
            {
                StartCoroutine(FinalFadeOut(currentCutscene.fadeDuration));
            }

            currentCutscene = null;
        }
    }

    private IEnumerator FinalFadeOut(float duration)
    {
        yield return new WaitForSeconds(0.2f);

        float startTime = Time.time;
        float endTime = startTime + duration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / duration;
            fadeCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
    }

    private void OnPlayableDirectorStarted(PlayableDirector director)
    {
        Debug.Log($"Cutscene {currentCutscene.cutsceneID} started");
    }

    private void DisableCutsceneObjects(CutsceneData cutscene)
    {
        if (cutscene.objectsToDisableAfterCutscene != null)
        {
            foreach (var obj in cutscene.objectsToDisableAfterCutscene)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    private bool HasCutscenePlayed(string cutsceneID)
    {
        return PlayerPrefs.HasKey($"Cutscene_{cutsceneID}_Played");
    }

    private void SetCutscenePlayed(string cutsceneID)
    {
        PlayerPrefs.SetInt($"Cutscene_{cutsceneID}_Played", 1);
        PlayerPrefs.Save();
    }

    public bool IsCutscenePlaying()
    {
        return isCutscenePlaying;
    }

    public void ResetAllCutsceneData()
    {
        foreach (var cutscene in cutscenes)
        {
            PlayerPrefs.DeleteKey($"Cutscene_{cutscene.cutsceneID}_Played");
        }
        PlayerPrefs.Save();
    }

    private void ResetPositionAndPlayNextCutscene(string cutsceneID, bool ignorePlayOnce)
    {
        if (cutsceneDict.TryGetValue(cutsceneID, out CutsceneData cutscene) &&
            cutscene.resetPlayerPosition &&
            cutscene.playerStartPosition != null &&
            playerController != null)
        {
            if (cutscene.fadeWhenResetting && fadeCanvasGroup != null)
            {
                StartCoroutine(FadeAndResetPosition(cutscene, cutsceneID, ignorePlayOnce));
            }
            else
            {
                Transform playerTransform = playerController.transform;
                playerTransform.position = cutscene.playerStartPosition.position;
                playerTransform.rotation = cutscene.playerStartPosition.rotation;

                PlayCutscene(cutsceneID, ignorePlayOnce);
            }
        }
        else
        {
            PlayCutscene(cutsceneID, ignorePlayOnce);
        }
    }

    private IEnumerator FadeAndResetPosition(CutsceneData cutscene, string cutsceneID, bool ignorePlayOnce)
    {
        float startTime = Time.time;
        float endTime = startTime + cutscene.fadeDuration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / cutscene.fadeDuration;
            fadeCanvasGroup.alpha = t;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        Transform playerTransform = playerController.transform;
        playerTransform.position = cutscene.playerStartPosition.position;
        playerTransform.rotation = cutscene.playerStartPosition.rotation;

        PlayCutscene(cutsceneID, ignorePlayOnce);

        yield return new WaitForSeconds(0.2f);

        startTime = Time.time;
        endTime = startTime + cutscene.fadeDuration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / cutscene.fadeDuration;
            fadeCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
    }
}