using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{

    private Vector3 mousePos;
    private Camera cam;
    private DistanceJoint2D distJoint;
    private LineRenderer lineRenderer;
    private bool canGrapple;
    private Vector3 tempPos;
    public Transform startPos;
    public float grappleLength;
    public bool hitPlatform;
    public LayerMask platformLayer;

    public bool grappling;
    void Start()
    {
        cam = Camera.main;
        distJoint = GetComponent<DistanceJoint2D>();
        distJoint.enabled = false;
        lineRenderer = GetComponent<LineRenderer>();
        canGrapple = true;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(startPos.position, mousePos, Mathf.Infinity, platformLayer);

        if(hit)
            hitPlatform = true;
        else
            hitPlatform = false;

        if(Input.GetMouseButtonDown(0) & canGrapple)
        {
            grappling = true;
            distJoint.enabled = true;
            distJoint.connectedAnchor = mousePos;

            tempPos = mousePos;
            lineRenderer.enabled = true;

            canGrapple = false;
            hitPlatform = false;
        }
        else if(Input.GetMouseButtonDown(0) || GetComponent<PlayerController>().jumpPressed)
        {
            grappling = false;
            distJoint.enabled = false;
            lineRenderer.enabled = false;
            canGrapple = true;
        }
        if(lineRenderer.enabled == true)
        {
            lineRenderer.SetPosition(0, startPos.position);
            lineRenderer.SetPosition(1, tempPos);
        }
    }
}
