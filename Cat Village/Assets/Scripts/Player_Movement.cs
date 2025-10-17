using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float jogSpeed = 5f;
    public float runSpeed = 8f;
    public float rotSpeed = 10f;
    public float jumpForce = 5f;
    public Animator animator;
    public float rayLength = 1.1f;

    public bool capsLockOn = false;

    public LayerMask groundLayer;
    public Rigidbody rb;
    public bool isGrounded = true;

    public int jumpPhase = 0; //1 = start jump, 2 = mid air, 3 = landing
    public float jumpStartTime;
    public float phase1Duration = 1.5f; // Duration of phase 1

    public bool startJumpTimer = false;

    public bool isColliding = false;
    public float collisionRayLength = 0.5f;

    // Get player Inventory script from other game object
    public Player_Inventory playerInventory;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        CheckGrounded();
    }

    void Update()
    {
        CheckCapsLock();
        //Movement();
        StopCollisionMovement();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isColliding)
        {
            Jump();
            Debug.Log("Pressed space and jumped!");
        }

        CheckGrounded();
        if (startJumpTimer)
        {
            UpdateJumpPhase();
            jumpStartTime += Time.deltaTime;
        }

        // Update holdTool animator boolean based on inventory
        if (playerInventory != null)
        {
            animator.SetBool("holdTool", playerInventory.isHoldingItem);
        }

        if (isGrounded)
        {
            Movement();
        }
        else if (!isGrounded)
        {
            AirMovement();
        }
    }

    void CheckCapsLock()
    {
        // Toggle capsLockOn when Caps Lock is pressed
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            capsLockOn = !capsLockOn;
        }
    }

    void Movement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float curSpeed;
            int movementPhase;

            if (Input.GetKey(KeyCode.LeftShift)) { curSpeed = runSpeed; movementPhase = 3; }
            else if (capsLockOn) { curSpeed = walkSpeed; movementPhase = 1; }
            else { curSpeed = jogSpeed; movementPhase = 2; }

            if (!isColliding)
            {
                transform.position += direction * curSpeed * Time.deltaTime;
                animator.SetInteger("MovementPhase", movementPhase);
            }
            else
            {
                animator.SetInteger("MovementPhase", 0);
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

            //animator.SetInteger("MovementPhase", movementPhase);
        }
        else
        {
            animator.SetInteger("MovementPhase", 0); // Idle
        }
    }

    void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    void CheckGrounded()
    {
        float radius = 0.3f;
        Vector3 origin = transform.position + Vector3.up * 0.05f; // just above feet

        // Visualize the sphere using Gizmos
        Debug.DrawLine(origin + Vector3.down * radius, origin + Vector3.down * (radius + 0.1f), Color.red);

        // Check if the sphere overlaps with ground colliders
        if (Physics.CheckSphere(origin, radius, groundLayer))
        {
            isGrounded = true;

            //reset all jump params once landed
            animator.SetBool("JumpStart", false);
            animator.SetBool("JumpMid", false);
            animator.SetBool("JumpEnd", false);
            jumpPhase = 0;
            startJumpTimer = false;
            jumpStartTime = 0;
        }
        else
        {
            isGrounded = false;
        }

        //while not grounded, play animations of jumping
        if (!isGrounded)
        {
            if (jumpPhase == 0)
            {
                //init set jumpPhase as 1
                jumpPhase = 1;
                startJumpTimer = true;
            }
            if (jumpPhase == 1)
            {
                animator.SetBool("JumpStart", true);
            }
            if (jumpPhase == 2)
            {
                animator.SetBool("JumpStart", false);
                animator.SetBool("JumpMid", true);
                jumpStartTime = 0;
            }
        }
    }

    void UpdateJumpPhase()
    {
        if (jumpPhase == 1)
        {
            //jumpStartTime = Time.time;
            // If phase 1 duration elapsed, move to phase 2
            if (jumpStartTime >= phase1Duration)
            {
                jumpPhase = 2;
                startJumpTimer = false;
                jumpStartTime = 0; //reset timer
            }
        }
    }

    void AirMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        float curSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) curSpeed = runSpeed;
        else if (capsLockOn) curSpeed = walkSpeed;
        else curSpeed = jogSpeed;

        // Move the player manually in air (not physics-based)
        transform.position += direction * curSpeed * Time.deltaTime;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        }
    }

    void StopCollisionMovement()
    {
        //method to stop movement when colliding with an object
        //send raycast forwards direction of movement

        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        Vector3 rayDirection = transform.forward;

        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, collisionRayLength))
        {
            isColliding = true;
            Debug.DrawLine(rayOrigin, hit.point, Color.red);
        }
        else
        {
            isColliding = false;
            Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * collisionRayLength, Color.green);
        }
    }

}
