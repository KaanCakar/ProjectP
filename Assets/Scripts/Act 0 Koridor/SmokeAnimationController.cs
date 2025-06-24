using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// SmokeAnimationController manages the animation of a mist effect based on the player's distance.
/// </summary>
public class SmokeAnimationController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public Transform mistTransform;
    public float minDistance = 2.0f;
    public float maxDistance = 15.0f;

    [Header("Animator Settings")]
    public Animator mistAnimator;
    public string animationParameterName = "AnimationProgress";

    [Header("Animation Range")]
    public float minAnimationValue = 0.0f;
    public float maxAnimationValue = 1.0f;

    private float currentDistance;
    private float normalizedProgress;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (mistAnimator == null)
            mistAnimator = GetComponent<Animator>();

        if (mistAnimator == null)
            Debug.LogError("Animator Component does not found.");

        if (player == null)
            Debug.LogError("Player references does not found.");
    }

    private void Update()
    {
        if (player == null || mistTransform == null || mistAnimator == null) return;

        currentDistance = Vector3.Distance(player.position, mistTransform.position);

        if (currentDistance <= minDistance)
        {
            normalizedProgress = maxAnimationValue;
        }
        else if (currentDistance >= maxDistance)
        {
            normalizedProgress = minAnimationValue;
        }
        else
        {
            float t = 1.0f - (currentDistance - minDistance) / (maxDistance - minDistance);
            normalizedProgress = Mathf.Lerp(minAnimationValue, maxAnimationValue, t);
        }

        mistAnimator.SetFloat(animationParameterName, normalizedProgress);

        Debug.Log($"Distance: {currentDistance:F2}m, Animation Progress: {normalizedProgress:F2}");
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Distance: {currentDistance:F2}m");
        GUI.Label(new Rect(10, 30, 300, 20), $"Animation Progress: {normalizedProgress:F2}");
    }
}