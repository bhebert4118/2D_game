using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float wallJumpForce = 7f;
    private float inputLockTimer = 0f;
    public float wallJumpControlDelay = 0.15f;

    // How long the player hangs completely still on the wall
    public float wallStickTime = 0f;

    // Downward speed after the hang ends
    public float wallSlideSpeed = 1.5f;

    private Rigidbody rb;

    private bool onGround = false;
    private bool touchingLeftWall = false;
    private bool touchingRightWall = false;

    private float wallJumpTimer = 0f;
    private float wallStickTimer = 0f;

    private bool wasTouchingWall = false;

    void Start()
    {
    rb = GetComponent<Rigidbody>();
    }
    public void LockInput(float duration)
    {
    inputLockTimer = duration;
    }
    void Update()
    {
    if (inputLockTimer > 0f)
    {
    inputLockTimer -= Time.deltaTime;
    return;
    }
    if (wallJumpTimer > 0f)
    {
    wallJumpTimer -= Time.deltaTime;

    // Holding space keeps the lock on
    if (Keyboard.current.spaceKey.isPressed && wallJumpTimer < 0.01f)
    {
    wallJumpTimer = 0.01f;
    }
    }

    // -------------------------
    // INPUT
    // -------------------------

    float horizontal = 0f;

    if (Keyboard.current.aKey.isPressed ||
        Keyboard.current.leftArrowKey.isPressed)
    {
    horizontal = -1f;
    }

    if (Keyboard.current.dKey.isPressed ||
        Keyboard.current.rightArrowKey.isPressed)
    {
    horizontal = 1f;
    }

    // -------------------------
    // NORMAL MOVEMENT
    // -------------------------

    if (wallJumpTimer <= 0f)
    {
    rb.linearVelocity = new Vector3(
        horizontal * moveSpeed,
        rb.linearVelocity.y,
        0f
    );
    }

    bool touchingWall = touchingLeftWall || touchingRightWall;

    // -------------------------
    // WALL STICK / SLIDE
    // -------------------------

    if (touchingWall && !onGround && wallJumpTimer <= 0f)
    {
    // Just reached the wall
    if (!wasTouchingWall)
    {
    wallStickTimer = wallStickTime;
    }

    // Don't start the cling until we've stopped moving upward
    if (rb.linearVelocity.y <= 0f)
    {
    if (wallStickTimer > 0f)
    {
    wallStickTimer -= Time.deltaTime;

    // COMPLETELY freeze vertical movement
    rb.linearVelocity = new Vector3(
        rb.linearVelocity.x,
        0f,
        0f
    );

    // Cancel gravity during the hang
    rb.useGravity = false;
    }
    else
    {
    // Hang is over
    rb.useGravity = true;

    // Force the wall-slide speed
    rb.linearVelocity = new Vector3(
        rb.linearVelocity.x,
        -wallSlideSpeed,
        0f
    );
    }
    }
    }
    else
    {
    // Make absolutely sure normal gravity is restored
    rb.useGravity = true;
    }

    // Reset cling time after actually leaving the wall
    if (!touchingWall)
    {
    wallStickTimer = wallStickTime;
    }

    wasTouchingWall = touchingWall;

    // -------------------------
    // JUMPING
    // -------------------------

    if (Keyboard.current.spaceKey.wasPressedThisFrame)
    {
    if (onGround)
    {
    rb.useGravity = true;

    rb.linearVelocity = new Vector3(
        rb.linearVelocity.x,
        jumpForce,
        0f
    );
    }
    else if (touchingLeftWall)
    {
    rb.useGravity = true;

    rb.linearVelocity = new Vector3(
        wallJumpForce,
        jumpForce,
        0f
    );

    wallJumpTimer = wallJumpControlDelay;
    wallStickTimer = 0f;
    wasTouchingWall = false;
    }
    else if (touchingRightWall)
    {
    rb.useGravity = true;

    rb.linearVelocity = new Vector3(
        -wallJumpForce,
        jumpForce,
        0f
    );

    wallJumpTimer = wallJumpControlDelay;
    wallStickTimer = 0f;
    wasTouchingWall = false;
    }
    }
    }

    void OnCollisionStay(Collision collision)
    {
    if (!collision.gameObject.CompareTag("Jumpable"))
    {
    return;
    }

    foreach (ContactPoint contact in collision.contacts)
    {
    Vector3 normal = contact.normal;

    // Ground
    if (normal.y > 0.5f)
    {
    onGround = true;
    }

    // Wall on left
    if (normal.x > 0.5f)
    {
    touchingLeftWall = true;
    }

    // Wall on right
    if (normal.x < -0.5f)
    {
    touchingRightWall = true;
    }
    }
    }

    void OnCollisionExit(Collision collision)
    {
    if (collision.gameObject.CompareTag("Jumpable"))
    {
    onGround = false;
    touchingLeftWall = false;
    touchingRightWall = false;

    rb.useGravity = true;
    }
    }
}