using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] float MaxSpringForce = 20f;
    [SerializeField] float MinSpringForce = 4.5f;
    [SerializeField] float DampingForce = 7f;
    [SerializeField] float MassScale = 4.5f;
    [SerializeField] float MaxGrappleDistance = 70f;
    [SerializeField] float GrapplingBoostSpeed = 1f;
    [SerializeField] LayerMask LayerToGrapple;
    [Header("Transforms")]
    [SerializeField] Transform TargetingPoint;
    [SerializeField] Transform PlayerTransform;
    
    Rigidbody rigidBody;
    LineRenderer lineRenderer;
    Transform StartingRaycastPoint;
    Vector3 grapplePoint;
    SpringJoint joint;
    Coroutine grapplingRoutine;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        StartingRaycastPoint = Camera.main.transform;
        rigidBody = GetComponentInParent<Rigidbody>();
    }
    private void Update()
    {
        if(IsGrappling())
           HandleGrapplingStatus();
        

        if ( Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartGrappling();
        }
        if ( Input.GetKeyUp(KeyCode.Mouse1))
        {
            StopGrappling();
        }
    }

    private void HandleGrapplingStatus()
    {
        Debug.DrawRay(transform.position, FindGrapplePointDirection());
        
        DrawRope();

        RotatePlayerTowards(FindGrapplePointDirection());

        if (Input.GetKeyDown(KeyCode.LeftShift))
            AddGrapplingSpeed();
        if (Input.GetKeyUp(KeyCode.LeftShift))
            AdjustSpringLength();

    }

    
  

    private void StartGrappling()
    {
        if (grapplingRoutine == null)
        {
            grapplingRoutine = StartCoroutine(LaunchGrappler());
        }
    }
    IEnumerator LaunchGrappler()
    {
        RaycastHit hitInfo;
        float CurrentFiringDistance=.1f;
        
        //Specify how many vertex the line should have
        lineRenderer.positionCount = 2;

        while (MaxGrappleDistance>CurrentFiringDistance)
        {
            Vector3 p_position = StartingRaycastPoint.position;
            Vector3 currentPoint = p_position + StartingRaycastPoint.forward * CurrentFiringDistance;

            RotatePlayerTowards(currentPoint - p_position);
            DrawRopeFiring(currentPoint);
            if (Physics.Raycast(p_position, StartingRaycastPoint.forward, out hitInfo,CurrentFiringDistance, LayerToGrapple))
            {
                grapplePoint = hitInfo.point;
                CreateJoint();
                break;
            }
            else
            {
                CurrentFiringDistance+=.5f;
                yield return null;
            }
        }
        //Task Failed
        if(MaxGrappleDistance<CurrentFiringDistance)
        {
            StopGrappling();
        }
       
    }
    private void StopGrappling()
    {
        //If is not grappling job is done :)
        //if (!IsGrappling()) return;

        lineRenderer.positionCount = 0;
        
        if (grapplingRoutine != null)
        {
            StopCoroutine(grapplingRoutine);
            grapplingRoutine = null;
        }

        StartCoroutine(RotateCourotine());
        Destroy(joint);
    }
    IEnumerator RotateCourotine()
    {
        for (int i = 0; i < 200; i++)
        {
            PlayerTransform.rotation = Quaternion.Lerp(PlayerTransform.rotation, Quaternion.Euler(0f, PlayerTransform.rotation.eulerAngles.y, 0f), i/200f);
            yield return null;
        }
    }
    private void CreateJoint()
    {
        joint = PlayerTransform.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;
        
        AdjustSpringLength();

        joint.spring = MinSpringForce;
        joint.massScale = MassScale;
        joint.damper =  DampingForce;
    }

    private void AdjustSpringLength()
    {
        //The distance grapple will try to keep from grapple point
        float distanceFromPoint = Vector3.Distance(PlayerTransform.position, grapplePoint);
        
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;
    }

    void DrawRopeFiring (Vector3 currentPoint)
    {
        //If your are not grappling dont draw a rope;
        //if (!joint) return;
        
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, currentPoint);
    }
    void DrawRope()
    {
        //If your are not grappling dont draw a rope;
        if (!joint) return;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
    private void RotatePlayerTowards(Vector3 Direction)
    {
        Quaternion LookAtRotation = Quaternion.LookRotation(Direction);
        PlayerTransform.rotation = Quaternion.RotateTowards(PlayerTransform.rotation, LookAtRotation, 300f * Time.deltaTime);
    }
    private void AddGrapplingSpeed()
    {
        joint.maxDistance = 0f;
        joint.minDistance = 0f;

        joint.spring = MaxSpringForce;
        rigidBody.AddForce(FindGrapplePointDirection() * GrapplingBoostSpeed, ForceMode.VelocityChange);
    }
    Vector3 FindGrapplePointDirection()
    {
        return (grapplePoint - transform.position).normalized;
    }
    public bool IsGrappling()
    {
        return joint;
    }
}

