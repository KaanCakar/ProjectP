using UnityEngine;
using System.Collections;


/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script manages the hallucination system in a corridor sequence 
/// </summary>

public class CorridorHallucinationController : MonoBehaviour
{
    [Header("Hallucination System")]
    [SerializeField] private HallucinationSystem hallucinationSystem;

    [Header("Corridor Sequence")]
    [SerializeField] private CorridorSequence corridorSequence;
    [SerializeField] private DistanceVignetteController vignetteController;

    [Header("Hallucination Phases")]
    [SerializeField] private Transform hallucinationStartPoint;
    [SerializeField] private float hallucinationStartDistance = 5f;
    [SerializeField] private float maxHallucinationBeforeChase = 60f;
    [SerializeField] private float hallucinationIncreaseRate = 15f;

    [Header("Chase Hallucination")]
    [SerializeField] private float chaseHallucinationMultiplier = 2f;
    [SerializeField] private float maxChaseDistance = 15f;
    [SerializeField] private float minChaseDistance = 3f;
    [SerializeField] private AnimationCurve chaseHallucinationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Transform player;
    private GameObject chaser;
    private bool isInHallucinationZone = false;
    private bool isChaseActive = false;
    private float baseHallucinationLevel = 0f;
    private bool hallucinationSystemWasActive = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (hallucinationSystem == null)
        {
            hallucinationSystem = FindObjectOfType<HallucinationSystem>();
        }

        if (corridorSequence == null)
        {
            corridorSequence = FindObjectOfType<CorridorSequence>();
        }

        if (hallucinationSystem != null)
        {
            hallucinationSystemWasActive = hallucinationSystem.enabled;
            //hallucinationSystem.enabled = false;
            hallucinationSystem.hallucinationLevel = 0f;
        }

        if (hallucinationStartPoint != null)
        {
            CreateHallucinationTrigger();
        }
    }

    private void CreateHallucinationTrigger()
    {
        GameObject trigger = new GameObject("HallucinationStartTrigger");
        trigger.transform.position = hallucinationStartPoint.position;
        trigger.transform.parent = this.transform;

        BoxCollider col = trigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(5f, 3f, 3f);

        HallucinationTrigger ht = trigger.AddComponent<HallucinationTrigger>();
        ht.controller = this;
    }

    private void Update()
    {
        if (hallucinationSystem == null) return;

        if (isInHallucinationZone && !isChaseActive)
        {
            UpdatePreChaseHallucination();
        }
        else if (isChaseActive && chaser != null)
        {
            UpdateChaseHallucination();
        }
    }

    private void UpdatePreChaseHallucination()
    {
        if (vignetteController != null)
        {
            float vignetteProgress = GetVignetteProgress();

            if (vignetteProgress > 0.8f)
            {
                float hallucinationProgress = (vignetteProgress - 0.8f) / 0.2f;
                float targetHallucination = hallucinationProgress * maxHallucinationBeforeChase;

                hallucinationSystem.hallucinationLevel = Mathf.Lerp(
                    hallucinationSystem.hallucinationLevel,
                    targetHallucination,
                    Time.deltaTime * hallucinationIncreaseRate
                );

                baseHallucinationLevel = hallucinationSystem.hallucinationLevel;
            }
        }
    }

    private void UpdateChaseHallucination()
    {
        float distance = Vector3.Distance(player.position, chaser.transform.position);

        float normalizedDistance = Mathf.InverseLerp(maxChaseDistance, minChaseDistance, distance);
        float curveValue = chaseHallucinationCurve.Evaluate(normalizedDistance);

        float targetHallucination = baseHallucinationLevel + (curveValue * chaseHallucinationMultiplier * 30f);
        targetHallucination = Mathf.Clamp(targetHallucination, baseHallucinationLevel, hallucinationSystem.maxHallucinationLevel);

        hallucinationSystem.hallucinationLevel = Mathf.Lerp(
            hallucinationSystem.hallucinationLevel,
            targetHallucination,
            Time.deltaTime * 3f
        );
    }

    public void StartHallucinationZone()
    {
        isInHallucinationZone = true;

        if (hallucinationSystem != null)
        {
            hallucinationSystem.enabled = true;
            hallucinationSystem.hallucinationDecreaseRate = 0f;
        }

        Debug.Log("Hallucination zone activated!");
    }

    public void OnChaserSpawned(GameObject spawned)
    {
        chaser = spawned;
        isChaseActive = true;

        if (hallucinationSystem != null)
        {
            hallucinationSystem.hallucinationDecreaseRate = 10f;
        }

        Debug.Log("Chase hallucination activated!");
    }

    public void OnSafezoneReached()
    {
        isChaseActive = false;

        StartCoroutine(FadeOutHallucination());
    }

    private IEnumerator FadeOutHallucination()
    {
        if (hallucinationSystem == null) yield break;

        hallucinationSystem.hallucinationDecreaseRate = 25f;

        while (hallucinationSystem.hallucinationLevel > 0.1f)
        {
            yield return null;
        }

        hallucinationSystem.hallucinationLevel = 0f;
        hallucinationSystem.hallucinationDecreaseRate = 5f;

        if (!hallucinationSystemWasActive)
        {
            hallucinationSystem.enabled = false;
        }
    }

    private float GetVignetteProgress()
    {
        if (vignetteController != null)
        {
            return vignetteController.CurrentProgress;
        }
        return 0f;
    }

    private void OnGUI()
    {
        if (showDebugInfo && hallucinationSystem != null)
        {
            GUI.Label(new Rect(10, 150, 400, 20), $"Hallucination Zone: {isInHallucinationZone}");
            GUI.Label(new Rect(10, 170, 400, 20), $"Chase Active: {isChaseActive}");
            GUI.Label(new Rect(10, 190, 400, 20), $"Base Hallucination: {baseHallucinationLevel:F1}");
            GUI.Label(new Rect(10, 210, 400, 20), $"Current Hallucination: {hallucinationSystem.hallucinationLevel:F1}");

            if (chaser != null)
            {
                float distance = Vector3.Distance(player.position, chaser.transform.position);
                GUI.Label(new Rect(10, 230, 400, 20), $"Chaser Distance: {distance:F1}m");
            }
        }
    }
}

public class HallucinationTrigger : MonoBehaviour
{
    public CorridorHallucinationController controller;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.StartHallucinationZone();
            Destroy(gameObject);
        }
    }
}