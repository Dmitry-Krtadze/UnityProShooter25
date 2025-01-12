using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject playerCamera;
    [SerializeField]
    private float
        walkSpeed, sprintSpeed, mouseSensitivity,
        jumpForce, smoothTime;
    private float vertacalLookRotation;
    [SerializeField]  private bool isGround;
    private Vector3 smothMove;
    private Vector3 moveAmout;
    private Rigidbody rb;
    private PhotonView pnView;

    private void Awake()
    {
        pnView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        if (!pnView.IsMine)
        {
            Destroy(playerCamera);
            Destroy(rb);
        }
    }
    private void Update()
    {
        if (!pnView.IsMine)
        {
            return;
        }
        Look();
        Movement();
        Jump();
    }
    private void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") *
            mouseSensitivity);
        vertacalLookRotation += Input.GetAxisRaw("Mouse Y") *
            mouseSensitivity;
        vertacalLookRotation = Mathf.Clamp(vertacalLookRotation,
            -80f, 90f);
        playerCamera.transform.localEulerAngles = Vector3.left *
            vertacalLookRotation;
    }
    private void Movement()
    {
        Vector3 moveDir = new Vector3(
            Input.GetAxisRaw("Horizontal"), 0,
            Input.GetAxisRaw("Vertical")).normalized;
        moveAmout = Vector3.SmoothDamp(moveAmout, moveDir *
            (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed :
            walkSpeed), ref smothMove, smoothTime);
    }
    private void FixedUpdate()
    {
        if (!pnView.IsMine)
        {
            return;
        }
        rb.MovePosition(rb.position + transform.TransformDirection(
            moveAmout) * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jump");
        }
        
    }

    public void GroundState(bool iSGround)
    {
        this.isGround = iSGround;
    }
}
