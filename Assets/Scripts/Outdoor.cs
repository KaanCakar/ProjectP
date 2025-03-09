using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outdoor : MonoBehaviour
{
    public GameObject wallFirst;
    public GameObject wallSecond;
    public GameObject colDelet;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            wallFirst.SetActive(false);
            wallSecond.SetActive(true);
            Destroy(colDelet);
        }
    }
}
