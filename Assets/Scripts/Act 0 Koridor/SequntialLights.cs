using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script manages a sequence of lights that turn on and off in pairs or individually.
/// </summary>
public class SequentialLights : MonoBehaviour
{
    [SerializeField] private Light[] lights;
    [SerializeField] private float timeBetweenLightPairs = 0.15f;
    [SerializeField] private float stayOnDuration = 2.0f;
    [SerializeField] private float initialDelay = 0.5f;

    [SerializeField] private bool loopEffect = false;
    [SerializeField] private Material[] emissionMaterials;
    [SerializeField] private float maxIntensity = 2.0f;
    [SerializeField] private AudioSource clickSound;
    [SerializeField] private bool useLightPairs = true;

    private float[] initialIntensities;
    private Color[] initialEmissionColors;
    private bool isRunning = false;

    private void Start()
    {
        initialIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            initialIntensities[i] = lights[i].intensity;
            lights[i].intensity = 0;
        }

        if (emissionMaterials != null && emissionMaterials.Length > 0)
        {
            initialEmissionColors = new Color[emissionMaterials.Length];
            for (int i = 0; i < emissionMaterials.Length; i++)
            {
                initialEmissionColors[i] = emissionMaterials[i].GetColor("_EmissionColor");
                emissionMaterials[i].SetColor("_EmissionColor", Color.black);
            }
        }
    }

    public void TriggerLightSequence()
    {
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(RunLightSequence());
        }
    }

    private IEnumerator RunLightSequence()
    {
        yield return new WaitForSeconds(initialDelay);

        do
        {
            if (useLightPairs)
            {
                int pairCount = lights.Length / 2;
                for (int i = 0; i < pairCount; i++)
                {
                    int firstIndex = i;
                    int secondIndex = i + pairCount;

                    if (secondIndex < lights.Length)
                    {
                        TurnOnLight(firstIndex);
                        TurnOnLight(secondIndex);

                        if (clickSound != null)
                            clickSound.Play();

                        yield return new WaitForSeconds(timeBetweenLightPairs);
                    }
                }
            }
            else
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    TurnOnLight(i);

                    if (clickSound != null)
                        clickSound.Play();

                    yield return new WaitForSeconds(timeBetweenLightPairs);
                }
            }

            yield return new WaitForSeconds(stayOnDuration);

            if (useLightPairs)
            {
                int pairCount = lights.Length / 2;
                for (int i = 0; i < pairCount; i++)
                {
                    int firstIndex = i;
                    int secondIndex = i + pairCount;

                    if (secondIndex < lights.Length)
                    {
                        TurnOffLight(firstIndex);
                        TurnOffLight(secondIndex);

                        if (clickSound != null)
                            clickSound.Play();

                        yield return new WaitForSeconds(timeBetweenLightPairs);
                    }
                }
            }
            else
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    TurnOffLight(i);

                    if (clickSound != null)
                        clickSound.Play();

                    yield return new WaitForSeconds(timeBetweenLightPairs);
                }
            }

            yield return new WaitForSeconds(1.0f);

        } while (loopEffect);

        isRunning = false;
    }

    private void TurnOnLight(int index)
    {
        if (index < 0 || index >= lights.Length) return;

        lights[index].intensity = maxIntensity;

        if (emissionMaterials != null && index < emissionMaterials.Length)
        {
            emissionMaterials[index].SetColor("_EmissionColor", initialEmissionColors[index] * 2.0f);
            emissionMaterials[index].EnableKeyword("_EMISSION");
        }
    }

    private void TurnOffLight(int index)
    {
        if (index < 0 || index >= lights.Length) return;

        lights[index].intensity = 0;

        if (emissionMaterials != null && index < emissionMaterials.Length)
        {
            emissionMaterials[index].SetColor("_EmissionColor", Color.black);
        }
    }
}