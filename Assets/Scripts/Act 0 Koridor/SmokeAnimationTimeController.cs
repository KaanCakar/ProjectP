using UnityEngine;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// SmokeAnimationTimeController manages the animation of a mist effect based on the player's distance.
/// </summary>
public class SmokeAnimationTimeController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public Transform mistTransform;
    public float minDistance = 2.0f;
    public float maxDistance = 15.0f;

    [Header("Animator Settings")]
    public Animator mistAnimator;
    public string stateName = "SmokeAnimation";
    public int layerIndex = 0;

    [Header("Animation Time Range")]
    [Range(0f, 1f)] public float minTimeNormalized = 0.3f;
    [Range(0f, 1f)] public float maxTimeNormalized = 0.7f;

    private float currentDistance;
    private float normalizedTime;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (mistAnimator == null)
            mistAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player == null || mistTransform == null || mistAnimator == null) return;

        currentDistance = Vector3.Distance(player.position, mistTransform.position);

        float normalizedDistance;

        if (currentDistance <= minDistance)
        {
            normalizedTime = 1.0f;
        }
        else if (currentDistance >= maxDistance)
        {
            normalizedTime = minTimeNormalized;
        }
        else
        {
            normalizedDistance = 1.0f - (currentDistance - minDistance) / (maxDistance - minDistance);

            normalizedTime = Mathf.Lerp(minTimeNormalized, maxTimeNormalized, normalizedDistance);

            if (currentDistance < minDistance + 0.5f && normalizedTime > maxTimeNormalized)
            {
                normalizedTime = 1.0f;
            }
        }

        mistAnimator.Play(stateName, layerIndex, normalizedTime);

        Debug.Log($"Distance: {currentDistance:F2}m, NormalizedTime: {normalizedTime:F2}");
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Distance: {currentDistance:F2}m");
        GUI.Label(new Rect(10, 30, 300, 20), $"Normalized Time: {normalizedTime:F2}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Second: {normalizedTime * 10:F1}s");
    }
}