using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Movement
    private Rigidbody2D rb2d;
    private float xAxis;
    public float moveSpeed;
    
    //Flip
    private bool facingRight = true;

    //Jump
    private bool isGrounded;
    private bool isTouchingGround;
    public bool jumpPressed = false;
    public Transform groundChecker;
    public float checkRadius = 0.5f;
    public LayerMask whatIsGround;
    public float jumpForce;

    //Wall Slide
    private bool isTouchingFront;
    private bool wallSliding;
    public Transform frontChecker;
    public LayerMask whatIsWall;
    public float frontCheckRadius;
    public float wallSlidingSpeed;
    
    //Wall Jump
    private float jumpTime;
    public float wallJumpTime;

    //Ledge Climb
    private bool isTouchingLedge;
    public Transform ledgeChecker;
    private bool ledgeGrabbing;
    private bool ledgeDetected;
    public Transform ledgePos;
    public float ledgeClimbOffset = 0f;
    private float gravity;
    
    //Animations
    private Animator animator;
    private string currentState;
    const string playerIdle = "PlayerIdle";
    const string playerRun = "PlayerRun";
    const string playerJump = "PlayerJump";
    const string playerFall = "PlayerFall";
    const string playerWallSlide = "PlayerWallSlide";
    const string playerLedgeGrab = "PlayerLedgeGrab";

    public float timeMultiplier;
    public bool timeSlowed;

    


    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gravity = rb2d.gravityScale;
    }
    

    void Update()
    {
        //Check if player is grounded
        isTouchingGround = Physics2D.OverlapCircle(groundChecker.position, checkRadius, whatIsGround);

        //Check if player is touching anything in front
        isTouchingFront = Physics2D.OverlapCircle(frontChecker.position, frontCheckRadius, whatIsWall);

        //Check if player is touching a ledge
        isTouchingLedge = Physics2D.OverlapCircle(ledgeChecker.position, frontCheckRadius, whatIsGround);
        
        //Ledge Grab --> Climb-Drop
        LedgeGrab();

        //Check x Axis input
        if(!ledgeGrabbing)
            xAxis = Input.GetAxisRaw("Horizontal");
        
        //Check jump input
        if(Input.GetButtonDown("Jump") && isGrounded || Input.GetButtonDown("Jump") && wallSliding || Input.GetButtonDown("Jump") && GetComponent<Grapple>().grappling)
            jumpPressed = true;

        //Flip Character
        if(xAxis < 0 & facingRight)
            FlipCharacter();
        else if (xAxis > 0 & !facingRight)
            FlipCharacter();
        
        //Give the player some time to jump if they just got off the platform
        if(isTouchingGround)
        {
            isGrounded = true;
            jumpTime = Time.time + wallJumpTime;
        }
        else if(jumpTime < Time.time)
            isGrounded = false;

        //Check if wall sliding
        if(isTouchingFront && !isGrounded && xAxis != 0 && !ledgeGrabbing)
        {
            wallSliding = true;   
            jumpTime = Time.time + wallJumpTime;       
        }
        else if(jumpTime < Time.time)
            wallSliding = false;

        //Slow time if F is pressed
        if(Input.GetKey(KeyCode.F))
        {
            /*Can grapple if F is pressed
            GetComponent<Grapple>().enabled = true;*/

            //Time is slowed and constant if the player isn't grappling
            if(!GetComponent<Grapple>().grappling)
            {    
                Time.timeScale = timeMultiplier;
                timeSlowed = true;
            }
            //Time changes if the player is grappling
            if(GetComponent<Grapple>().grappling)
            {
                //If grapple is thrown before F is pressed slow the time
                if(!timeSlowed)
                {
                    Time.timeScale = timeMultiplier;
                    timeSlowed = true;
                }
                //Time speeds up when the player is falling while grappling
                if(rb2d.velocity.y < 0)
                {    
                    if(Time.timeScale < 1)
                        Time.timeScale += 2f * Time.deltaTime;
                    else
                        Time.timeScale = 1;    
                }
                //Time slows down when the player is rising while grappling 
                if(rb2d.velocity.y > 0)
                {
                    if(Time.timeScale > 0.1f)
                        Time.timeScale -= 2f * Time.deltaTime;
                    else
                        Time.timeScale = 0.1f;     
                }    
            }
        }
        else
        {
            /*Cannot grapple if F is not pressed
            GetComponent<Grapple>().enabled = false;
            GetComponent<DistanceJoint2D>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            */
            Time.timeScale = 1;
            timeSlowed = false;
        }


        //Change Animations  
        if(!ledgeGrabbing)    
            ChangeAnimations();

        Debug.Log(Time.timeScale);        
    }

    void FixedUpdate() 
    {
        //Movement
        if(!ledgeGrabbing && !GetComponent<Grapple>().grappling)
            Move();

        //Jump
        if(jumpPressed)
            Jump();       

        //Wall Slide
        if(wallSliding)
        {   
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Clamp(rb2d.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
    }

    
    //Movement
    void Move()
    {    
        rb2d.velocity = new Vector2(xAxis * moveSpeed, rb2d.velocity.y);
    }
    void Jump()
    {
        jumpPressed = false;
        rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
    }
    void LedgeGrab()
    {
        if(isTouchingFront && !isTouchingLedge && !ledgeDetected)
            {
                ledgeDetected = true;
            }
            if(ledgeDetected)
            {
                ledgeGrabbing = true;
                rb2d.velocity = new Vector2(0,0);
                rb2d.gravityScale = 0;
                ChangeAnimationState(playerLedgeGrab);
                transform.position = new Vector3(transform.position.x, transform.position.y -ledgeClimbOffset, transform.position.z);
                ledgeDetected = false;
            }
            if(ledgeGrabbing && Input.GetKeyDown(KeyCode.Space))
            {
                ledgeGrabbing = false;
                rb2d.gravityScale = gravity;
                transform.position = ledgePos.position;
            }
            if(ledgeGrabbing && Input.GetKeyDown(KeyCode.DownArrow))
            {
                ledgeGrabbing = false;
                rb2d.gravityScale = gravity;
            }
    }
    void FlipCharacter()
    {
        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        facingRight = !facingRight;
    }

    //Animations
    void ChangeAnimations()
    {
        //Run, Idle, Fall Animations
        if(isTouchingGround)
        {
            if(xAxis != 0)
                ChangeAnimationState(playerRun);
            else
                ChangeAnimationState(playerIdle);
        }
        else if((rb2d.velocity.y < 0 && !isTouchingFront && !isTouchingGround) || (isTouchingFront && !isTouchingGround && xAxis == 0))
        {
            ChangeAnimationState(playerFall);
        }

        //Wall Slide and Jump Animations
        if(isTouchingFront && !isTouchingGround && xAxis != 0)
            ChangeAnimationState(playerWallSlide);
        if(rb2d.velocity.y > 0 && !isTouchingFront)
            ChangeAnimationState(playerJump);  
    }
    void ChangeAnimationState(string newState)
    {
        if(currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }
    
    void OnDrawGizmos() 
    {
        Gizmos.DrawWireSphere(groundChecker.position, checkRadius);
        Gizmos.DrawWireSphere(frontChecker.position, frontCheckRadius);
        Gizmos.DrawWireSphere(ledgeChecker.position, frontCheckRadius);
    }
}
