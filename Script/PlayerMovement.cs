using UnityEngine;

/// <summary>
/// Handles player movement including walking, jumping, and crouching mechanics
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    #region Components
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    #endregion

    #region Movement Settings
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;
    private float moveInput;
    private bool facingRight = true;
    #endregion

    #region Jump Settings
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    #endregion

    #region Ground Detection
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    #endregion

    #region Wall Detection
    [Header("Wall Detection")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private float wallCheckRadius = 0.1f;
    private bool isTouchingWall;
    #endregion

    #region Crouch Settings
    [Header("Crouch")]
    [SerializeField] private float crouchSpeedMultiplier = 0.05f;
    [SerializeField] private float crouchHeightMultiplier = 0.5f;
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private float ceilingCheckRadius = 0.2f;
    [SerializeField] private float crouchDuration = 3f;
    
    private bool isCrouching;
    private bool crouchToggleActive = false;
    private float crouchTimer = 0f;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector3 originalScale;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeComponents();
        StoreOriginalValues();
    }
    
    /// <summary>
    /// Initialize all required components
    /// </summary>
    private void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        if (anim == null) Debug.LogError("Animator component not found!");
        if (rb == null) Debug.LogError("Rigidbody2D component not found!");
        if (boxCollider == null) Debug.LogError("BoxCollider2D component not found!");
    }
    
    /// <summary>
    /// Store original values for resetting
    /// </summary>
    private void StoreOriginalValues()
    {
        originalColliderSize = boxCollider.size;
        originalColliderOffset = boxCollider.offset;
        originalScale = transform.localScale;
    }

    void Update()
    {
        HandleInput();
        UpdateAnimations();
        HandleJumping();
        HandleCrouching();
        CheckWallCollision();
    }
    
    /// <summary>
    /// Handle all input processing
    /// </summary>
    private void HandleInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = IsGrounded();
        
        // Handle facing direction
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }
    
    /// <summary>
    /// Update animation parameters
    /// </summary>
    private void UpdateAnimations()
    {
        if (anim == null) return;
        
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetBool("isGrounded", isGrounded);
        
        // Update jumping animation
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            anim.SetBool("isJumping", false);
        }
        else if (!isGrounded && rb.linearVelocity.y > 0.1f)
        {
            anim.SetBool("isJumping", true);
        }
    }
    
    /// <summary>
    /// Handle jumping mechanics with buffer system
    /// </summary>
    private void HandleJumping()
    {
        // Jump Buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Jump Logic
        if (jumpBufferCounter > 0)
        {
            if (isGrounded)
            {
                Jump();
                if (anim != null) anim.SetBool("isJumping", true);
                jumpBufferCounter = 0f;
            }
            else if (crouchToggleActive)
            {
                // Infinite jumps while crouching
                Jump();
                jumpBufferCounter = 0f;
            }
        }
    }
    
    /// <summary>
    /// Handle crouching mechanics
    /// </summary>
    private void HandleCrouching()
    {
        // Start crouching
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !crouchToggleActive)
        {
            StartCrouching();
        }
        
        // Handle crouch duration
        if (crouchToggleActive)
        {
            crouchTimer -= Time.deltaTime;
            
            if (crouchTimer <= 0f)
            {
                TryStandUp();
            }
        }
        else
        {
            rb.gravityScale = 3f;
        }
    }
    
    /// <summary>
    /// Check for wall collisions
    /// </summary>
    private void CheckWallCollision()
    {
        if (wallCheckLeft != null && wallCheckRight != null)
        {
            bool leftWall = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, groundLayer);
            bool rightWall = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, groundLayer);
            isTouchingWall = leftWall || rightWall;
        }
    }

        void FixedUpdate()
    {
        HandleMovement();
    }
    
    /// <summary>
    /// Handle horizontal movement with crouch speed modifier
    /// </summary>
    private void HandleMovement()
    {
        float speed = moveSpeed * (crouchToggleActive ? crouchSpeedMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    #endregion // <-- This closes the "Unity Lifecycle" region

    #region Movement Methods

    
    /// <summary>
    /// Flip the player sprite to face the opposite direction
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    /// <summary>
    /// Execute jump with different behavior based on crouch state
    /// </summary>
    private void Jump()
    {
        if (crouchToggleActive)
        {
            // Stay in crouch but still allow upward boost (infinite jump style)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            return;
        }
        
        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    /// <summary>
    /// Check if the player is grounded
    /// </summary>
    /// <returns>True if player is on ground</returns>
    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }
    
    #endregion
    
    #region Crouch Methods
    
    /// <summary>
    /// Start the crouching state
    /// </summary>
    private void StartCrouching()
    {
        crouchToggleActive = true;
        crouchTimer = crouchDuration;
        
        boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * crouchHeightMultiplier);
        boxCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y * crouchHeightMultiplier);
        
        if (anim != null) anim.SetBool("isCrouching", true);
    }
    
    /// <summary>
    /// Try to stand up from crouch position
    /// </summary>
    private void TryStandUp()
    {
        bool canStandUp = !Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
        
        if (canStandUp)
        {
            EndCrouching();
        }
        // else: remain crouched until there's space above
    }

    /// <summary>
    /// End the crouching state
    /// </summary>
    private void EndCrouching()
    {
        crouchToggleActive = false;
        boxCollider.size = originalColliderSize;
        boxCollider.offset = originalColliderOffset;

        if (anim != null) anim.SetBool("isCrouching", false);
        rb.gravityScale = 3f;
    }
    
    #endregion
    
}