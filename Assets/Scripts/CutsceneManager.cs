using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

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
        [Header("Camera Settings")]
        public bool useCutsceneCamera = false; 
        public Camera cutsceneCamera;           
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

    private Dictionary<string, CutsceneData> cutsceneDict = new Dictionary<string, CutsceneData>();
    private CutsceneData currentCutscene;
    private bool isCutscenePlaying = false;
    private bool wasPlayerControlEnabled = false;
    private CursorLockMode previousCursorState;

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

    public void PlayCutscene(string cutsceneID)
    {
        if (isCutscenePlaying) return;

        if (cutsceneDict.TryGetValue(cutsceneID, out CutsceneData cutscene))
        {
            if (cutscene.playOnce && HasCutscenePlayed(cutsceneID))
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

    private void StartCutscene()
    {
        if (currentCutscene == null || currentCutscene.timelineDirector == null) return;

        isCutscenePlaying = true;

        wasPlayerControlEnabled = playerController != null && playerController.enabled;
        previousCursorState = Cursor.lockState;

        if (currentCutscene.disablePlayerControl && playerController != null)
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

        currentCutscene.timelineDirector.played += OnPlayableDirectorStarted;
        currentCutscene.timelineDirector.stopped += OnPlayableDirectorStopped;

        currentCutscene.onCutsceneStart?.Invoke();

        OnCutsceneStarted?.Invoke();

        currentCutscene.timelineDirector.Play();
    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (currentCutscene == null) return;

        if (currentCutscene.useCutsceneCamera && currentCutscene.cutsceneCamera != null)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);
                
            currentCutscene.cutsceneCamera.gameObject.SetActive(false);
        }

        if (currentCutscene.disablePlayerControl && playerController != null)
        {
            playerController.enabled = wasPlayerControlEnabled;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
        currentCutscene = null;
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
}