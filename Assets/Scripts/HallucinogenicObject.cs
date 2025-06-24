using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kaan ÇAKAR theanswer! - 2025
/// This script defines a hallucinogenic object that increases the player's hallucination level when interacted with.
/// </summary>

[System.Serializable]
public class HallucinogenicObject : MonoBehaviour
{
    public string objectName;
    public float hallucinationIncrease = 10f; 
    public string description = "This object induces hallucinations when interacted with.";
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}