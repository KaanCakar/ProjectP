using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outdoor : MonoBehaviour
{
    [SerializeField] private SequentialLights lightSequence;
    public GameObject wallFirst;
    public GameObject wallSecond;
    public GameObject[] openObjects;
    public GameObject[] closeObjects;
    public GameObject colDelet;

    [Header("Oyuncu Bakış Kontrolü")]
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float requiredLookTime = 2.0f;
    [SerializeField] private float lookAngleThreshold = 30f;

    private Transform playerTransform;
    private bool isPlayerInArea = false;
    private bool hasTriggered = false;
    private float currentLookTime = 0f;

    [SerializeField] private DistanceVignetteController vignetteController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player" && !hasTriggered)
        {
            isPlayerInArea = true;
            playerTransform = other.transform;

            wallFirst.SetActive(false);
            wallSecond.SetActive(true);
            foreach (GameObject obj in openObjects)
            {
                obj.SetActive(true);
            }

            foreach (GameObject obj in closeObjects)
            {
                obj.SetActive(false);
            }

            if (vignetteController != null)
            {
                vignetteController.Activate();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            isPlayerInArea = false;
            playerTransform = null;
            currentLookTime = 0f;
        }
    }

    private void Update()
    {
        if (isPlayerInArea && !hasTriggered && playerTransform != null && lookTarget != null)
        {
            Vector3 directionToTarget = (lookTarget.position - playerTransform.position).normalized;
            float angle = Vector3.Angle(playerTransform.forward, directionToTarget);

            if (angle < lookAngleThreshold)
            {
                currentLookTime += Time.deltaTime;

                if (currentLookTime >= requiredLookTime)
                {
                    lightSequence.TriggerLightSequence();
                    hasTriggered = true;
                    Destroy(colDelet);
                }
            }
            else
            {
                currentLookTime = 0f;
            }
        }
    }
}