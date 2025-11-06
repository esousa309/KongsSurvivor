using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float acceleration = 30f;
    public float deceleration = 40f;
    public float maxAirControl = 0.6f;
    
    [Header("Ground Check")]
    public float gravity = -9.81f;
    public float jumpHeight = 0f; // Set to 0 for top-down, increase for platformer
    public LayerMask groundMask = -1;
    public float groundCheckRadius = 0.3f;
    public float groundCheckOffset = 0.05f;
    
    [Header("Camera Reference")]
    public Transform cameraTransform;
    
    private Rigidbody rb3D;
    private Rigidbody2D rb2D;
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isGrounded = true;
    private bool is3D = false;
    
    void Start()
    {
        // Check if using 3D or 2D physics
        rb3D = GetComponent<Rigidbody>();
        rb2D = GetComponent<Rigidbody2D>();
        
        if (rb3D != null)
        {
            is3D = true;
            rb3D.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else if (rb2D == null)
        {
            // Add 2D rigidbody if none exists
            rb2D = gameObject.AddComponent<Rigidbody2D>();
            rb2D.gravityScale = 0f; // Top-down view
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // Find camera if not assigned
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }
    }
    
    void Update()
    {
        // Get input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        
        // Normalize diagonal movement
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
        
        // Jump input (if jump is enabled)
        if (jumpHeight > 0 && isGrounded && Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }
    
    void FixedUpdate()
    {
        if (is3D)
        {
            Move3D();
        }
        else
        {
            Move2D();
        }
    }
    
    void Move2D()
    {
        if (rb2D == null) return;
        
        Vector2 targetVelocity = moveInput * moveSpeed;
        
        // Smooth movement
        Vector2 currentVelocity = rb2D.velocity;
        float smoothing = moveInput.magnitude > 0.1f ? acceleration : deceleration;
        
        rb2D.velocity = Vector2.Lerp(currentVelocity, targetVelocity, smoothing * Time.fixedDeltaTime);
    }
    
    void Move3D()
    {
        if (rb3D == null) return;
        
        // Ground check for 3D
        isGrounded = Physics.CheckSphere(
            transform.position - Vector3.up * groundCheckOffset,
            groundCheckRadius,
            groundMask
        );
        
        // Calculate movement
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        
        // Camera-relative movement
        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            
            move = forward * moveInput.y + right * moveInput.x;
        }
        
        // Apply movement
        Vector3 targetVelocity = move * moveSpeed;
        
        if (!isGrounded)
        {
            // Air control
            targetVelocity *= maxAirControl;
        }
        
        // Preserve Y velocity for gravity
        targetVelocity.y = rb3D.velocity.y;
        
        // Apply gravity
        if (!isGrounded)
        {
            targetVelocity.y += gravity * Time.fixedDeltaTime;
        }
        
        // Smooth movement
        float smoothing = moveInput.magnitude > 0.1f ? acceleration : deceleration;
        rb3D.velocity = Vector3.Lerp(rb3D.velocity, targetVelocity, smoothing * Time.fixedDeltaTime);
    }
    
    void Jump()
    {
        if (is3D && rb3D != null)
        {
            float jumpVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
            rb3D.velocity = new Vector3(rb3D.velocity.x, jumpVelocity, rb3D.velocity.z);
        }
        else if (rb2D != null && jumpHeight > 0)
        {
            // For 2D platformer-style jump
            rb2D.velocity = new Vector2(rb2D.velocity.x, Mathf.Sqrt(-2f * gravity * jumpHeight));
        }
    }
    
    public void AddKnockback(Vector3 force)
    {
        if (is3D && rb3D != null)
        {
            rb3D.AddForce(force, ForceMode.Impulse);
        }
        else if (rb2D != null)
        {
            rb2D.AddForce(new Vector2(force.x, force.y), ForceMode2D.Impulse);
        }
    }
    
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            // Stop movement when disabled
            if (rb3D != null)
                rb3D.velocity = Vector3.zero;
            else if (rb2D != null)
                rb2D.velocity = Vector2.zero;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw ground check sphere
        if (jumpHeight > 0)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(
                transform.position - Vector3.up * groundCheckOffset,
                groundCheckRadius
            );
        }
    }
}