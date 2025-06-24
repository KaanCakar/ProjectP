using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorChaser : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 1.5f;
    public float minFollowDistance = 5.0f;

    private void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > minFollowDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * followSpeed * Time.deltaTime;
        }
    }
    
    public void StopChasing()
    {
        // Optionally, you can disable the chaser or stop its movement
        enabled = false;
        // Or you can set followSpeed to 0 if you want to keep the component active
        // followSpeed = 0f;
    }
}