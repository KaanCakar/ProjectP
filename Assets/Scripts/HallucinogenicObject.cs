using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HallucinogenicObject : MonoBehaviour
{
    public string objectName;
    public float hallucinationIncrease = 10f; 
    public string description = "Bu obje sanrı seviyesini artırır";
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}