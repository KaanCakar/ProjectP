using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outdoor : MonoBehaviour
{
    public GameObject wallFirst;
    public GameObject wallSecond;
    public GameObject[] openObjects;
    public GameObject[] closeObjects;
    public GameObject colDelet;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
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
            Destroy(colDelet);
        }
    }
}
