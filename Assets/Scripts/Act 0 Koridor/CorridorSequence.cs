using UnityEngine;
using System.Collections;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script manages the corridor sequence, including triggering sirens, spawning chasers
/// </summary>
public class CorridorSequence : MonoBehaviour
{
    [Header("Sequence Triggers")]
    [SerializeField] private Transform sirenTriggerPoint;
    [SerializeField] private Transform chaserSpawnPoint;
    [SerializeField] private Transform safezonePoint;

    [Header("References")]
    [SerializeField] private SirenLight sirenLight;
    [SerializeField] private GameObject chaserPrefab;
    [SerializeField] private Transform chaserSpawnPosition;
    [SerializeField] private CanvasGroup dangerPanel;

    [Header("Chase Settings")]
    [SerializeField] private float dangerDistance = 3f;
    [SerializeField] private float maxDangerDistance = 10f;
    [SerializeField] private AnimationCurve dangerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip chaseMusic;

    private Transform player;
    private GameObject spawnedChaser;
    private CorridorChaser chaserController;
    private bool sirenTriggered = false;
    private bool chaserSpawned = false;
    private bool inSafezone = false;
    private CorridorHallucinationController hallucinationController;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (dangerPanel != null)
            dangerPanel.alpha = 0f;

        hallucinationController = FindObjectOfType<CorridorHallucinationController>();

        SetupTriggers();
    }

    private void SetupTriggers()
    {
        // Siren trigger
        if (sirenTriggerPoint != null)
        {
            CreateTrigger(sirenTriggerPoint, "SirenTrigger", TriggerType.Siren);
        }

        // Chaser spawn trigger
        if (chaserSpawnPoint != null)
        {
            CreateTrigger(chaserSpawnPoint, "ChaserTrigger", TriggerType.Chaser);
        }

        // Safezone trigger
        if (safezonePoint != null)
        {
            CreateTrigger(safezonePoint, "SafezoneTrigger", TriggerType.Safezone);
        }
    }

    private void CreateTrigger(Transform point, string name, TriggerType type)
    {
        GameObject trigger = new GameObject(name);
        trigger.transform.position = point.position;
        trigger.transform.parent = point;

        BoxCollider col = trigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(5f, 3f, 3f);

        SequenceTrigger st = trigger.AddComponent<SequenceTrigger>();
        st.sequence = this;
        st.type = type;
    }

    private void Update()
    {
        if (chaserSpawned && spawnedChaser != null && !inSafezone)
        {
            UpdateDangerUI();
        }
    }

    private void UpdateDangerUI()
    {
        if (dangerPanel == null || spawnedChaser == null) return;

        float distance = Vector3.Distance(player.position, spawnedChaser.transform.position);
        float normalizedDistance = Mathf.InverseLerp(maxDangerDistance, dangerDistance, distance);
        float targetAlpha = dangerCurve.Evaluate(normalizedDistance);

        dangerPanel.alpha = Mathf.Lerp(dangerPanel.alpha, targetAlpha, Time.deltaTime * 5f);

        if (distance < dangerDistance)
        {
            OnPlayerCaught();
        }
    }

    public void TriggerSiren()
    {
        if (sirenTriggered) return;
        sirenTriggered = true;

        if (sirenLight != null)
        {
            sirenLight.TriggerDangerMode();
        }

        Debug.Log("Siren activated - Danger mode!");
    }

    public void SpawnChaser()
    {
        if (chaserSpawned) return;
        chaserSpawned = true;

        if (chaserPrefab != null && chaserSpawnPosition != null)
        {
            spawnedChaser = Instantiate(chaserPrefab, chaserSpawnPosition.position,
                                       chaserSpawnPosition.rotation);
            chaserController = spawnedChaser.GetComponent<CorridorChaser>();

            // Notify hallucination controller
            if (hallucinationController != null)
            {
                hallucinationController.OnChaserSpawned(spawnedChaser);
            }

            if (ambienceSource != null && chaseMusic != null)
            {
                ambienceSource.clip = chaseMusic;
                ambienceSource.Play();
            }

            Debug.Log("Chaser spawned! RUN!");
        }
    }

    public void EnterSafezone()
    {
        if (inSafezone) return;
        inSafezone = true;

        if (chaserController != null)
        {
            chaserController.StopChasing();
        }

        // Notify hallucination controller
        if (hallucinationController != null)
        {
            hallucinationController.OnSafezoneReached();
        }

        StartCoroutine(FadeDangerUI());

        Debug.Log("Safe zone reached!");
    }

    private IEnumerator FadeDangerUI()
    {
        float elapsed = 0f;
        float startAlpha = dangerPanel.alpha;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            dangerPanel.alpha = Mathf.Lerp(startAlpha, 0f, elapsed);
            yield return null;
        }

        dangerPanel.alpha = 0f;
    }

    private void OnPlayerCaught()
    {
        Debug.Log("Player caught by creature!");
        // Buraya ölüm/restart mekanizması eklenebilir
    }

    public enum TriggerType
    {
        Siren,
        Chaser,
        Safezone
    }

    [System.Serializable]
    public class SequenceTrigger : MonoBehaviour
    {
        public CorridorSequence sequence;
        public TriggerType type;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                switch (type)
                {
                    case TriggerType.Siren:
                        sequence.TriggerSiren();
                        break;
                    case TriggerType.Chaser:
                        sequence.SpawnChaser();
                        break;
                    case TriggerType.Safezone:
                        sequence.EnterSafezone();
                        break;
                }
            }
        }
    }
}