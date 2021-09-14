using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof( Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Speed")]
    public float GroundSpeed = 30f;
    public float AirSpeed = 5f;
    public float MaxSpeed = 30f;
    
    [Header("Gravity")]
    public float ExtraGravity = 10f;
    public float jumpHeight = 10f;
    public float turnSmoothTime = 0.1f;
    public bool isGrounded = false;

    [Header("Drag")]
    [SerializeField] float groundCheckingDistance = 0.3f;
    public float DragGround = 4.5f;
    public float DragAir = 1f;

    [Header("Ground Detection")]
    [SerializeField] LayerMask LayerToCollide;
    [SerializeField] float SphereRadiusCollision= 0.2f;

    Transform PlayerMainCamera;
    CapsuleCollider capsuleCollider;
    Animator _animator;
    Rigidbody _rigidbody;
    Grappling grappling;
    float turnSmoothVelocity;
    bool canMove = true;
    private float playerHeight;
    private Vector3 playerCenter;
    bool jumped=false;

    private void Awake()
    {
        PlayerMainCamera = Camera.main.transform;
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        grappling = GetComponentInChildren<Grappling>();
        playerHeight = capsuleCollider.height;
        playerCenter = capsuleCollider.center;
    }

    void Update()
    {

        if (!canMove) return;
        
        ManageGravity();

        SpeedPlayer();
        
        MovePlayer();
    }
    


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,SphereRadiusCollision);
    }
    private void ManageGravity()
    {
        isGrounded =Physics.CheckSphere(transform.position , SphereRadiusCollision, LayerToCollide);

        //ExtraGravity
        if(isGrounded)
        {
            _rigidbody.AddForce(Vector3.down * ExtraGravity*Time.deltaTime);
            _animator.SetBool("Jumping", false);
        }

        ControllDrag();


        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetBool("Jumping", true);
            _rigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            isGrounded = false;
        }
        _animator.SetBool("Falling", !isGrounded);
    }

    private void MovePlayer()
    {
        //Reading from the keyboard where to go
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        
        if (direction.magnitude < 0.1f)
        {
            _animator.SetBool("Running", false);
            
            if ( isGrounded)
            _rigidbody.velocity = new Vector3(0f,_rigidbody.velocity.y,0f);
            
            return;
        }
        _animator.SetBool("Running", true);

        float targetAngle = SetNewRotationPlayer(direction);
        
        Vector3 moveDir = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized *  Time.deltaTime;
        if (isGrounded)
        {
            moveDir *= GroundSpeed;
        }
        else if (grappling.IsGrappling())
        {
            moveDir *= 3f * AirSpeed;
        }

        _rigidbody.AddForce(moveDir, ForceMode.VelocityChange);
    }

    private float SetNewRotationPlayer(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + PlayerMainCamera.eulerAngles.y;

        //Dont Update the rotation the if the player is currently grappling
        if (!grappling.IsGrappling())
        {
            float smoothangle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothangle, 0f);
        }

        return targetAngle;
    }

    private void SpeedPlayer()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            GroundSpeed *= 3;
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            GroundSpeed /= 3;
        if (_rigidbody.velocity.magnitude > MaxSpeed)
            _rigidbody.velocity = _rigidbody.velocity.normalized* MaxSpeed;
    }

    private void ControllDrag()
    {
        _rigidbody.drag = (isGrounded) ? DragGround : DragAir;
    }
    
 
}
