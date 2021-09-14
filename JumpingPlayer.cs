using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingPlayer : MonoBehaviour
{
    [SerializeField] Transform StartingRaycast;
    [SerializeField] LayerMask LayerToCollide;
    PlayerMovement playerMovement;
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
         
       //if (Input.GetKeyDown(KeyCode.Space) CheckIfGrounded();

    }

    private void CheckIfGrounded()
    {
        if (Physics.BoxCast(StartingRaycast.position, Vector3.one, Vector3.down, out RaycastHit hitInfo, Quaternion.identity, 5f, LayerToCollide))
        {
            Debug.Log($"We hit {hitInfo.collider.name}");
        }
    }

    
}
