using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class FollowWaypoints : MonoBehaviour
{
    [SerializeField]
    private Transform[] waypoints;

    [SerializeField]
    private float moveSpeed = 2f;
    private int waypointIndex = 0;

    private Light2D trailLight;


    void Start()
    {
        transform.position = waypoints[waypointIndex].transform.position;
        trailLight = this.GetComponent<Light2D>();
        //trailLight.enabled = true;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if(waypointIndex <= waypoints.Length -1)
        {
            transform.position = Vector2.MoveTowards(transform.position, waypoints[waypointIndex].transform.position, moveSpeed * Time.deltaTime);
        
            if(transform.position == waypoints[waypointIndex].transform.position)
            {
                waypointIndex += 1;
            }
        }
        else
        {
            this.gameObject.SetActive(false);
        }

    }
}
