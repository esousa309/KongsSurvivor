using UnityEngine;

/// <summary>
/// 3D top-down player controller (XZ movement + gravity on Y).
/// Works with a Rigidbody (recommended).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;          // target ground speed
    public float acceleration = 30f;      // how fast we reach target speed
    public float deceleration = 40f;      // how fast we stop when no input
    public float maxAirControl = 0.6f;    // fraction of control while in air

    [Header("Physics")]
    public float gravity = -9.81f;
    public float jumpHeight = 0f;         // set >0 if you want jump later
    public LayerMask groundMask = ~0;
    public float groundCheckRadius = 0.25f;
    public float groundCheckOffset = 0.05f;

    [Header("References")]
    public Transform cameraTransform;     // optional; if null we use Camera.main

    Rigidbody rb;
    Vector3 velocity;     // current velocity (world)
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // allow full XZ motion
        rb.useGravity = false; // we apply custom gravity for stable control

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // --- Ground check ---
        isGrounded = Physics.CheckSphere(transform.position + Vector3.up * groundCheckOffset, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        // --- Read input (WASD / arrow keys) ---
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");

        // Camera-relative on XZ (since camera is bird's-eye, this is effectively world XZ)
        Vector3 forward = Vector3.forward;
        Vector3 right   = Vector3.right;
        if (cameraTransform != null)
        {
            // project camera axes onto XZ plane to be safe
            Vector3 camF = cameraTransform.forward; camF.y = 0f; camF.Normalize();
            Vector3 camR = cameraTransform.right;   camR.y = 0f; camR.Normalize();
            if (camF.sqrMagnitude > 0.0001f) forward = camF;
            if (camR.sqrMagnitude > 0.0001f) right   = camR;
        }

        Vector3 desiredMove = (right * ix + forward * iz);
        if (desiredMove.sqrMagnitude > 1f) desiredMove.Normalize();
        desiredMove *= moveSpeed;

        // --- Horizontal (XZ) acceleration/deceleration ---
        Vector3 horizVel = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 desiredHoriz = new Vector3(desiredMove.x, 0f, desiredMove.z);

        float accel = (desiredHoriz.sqrMagnitude > 0.001f)
            ? acceleration
            : deceleration;

        if (!isGrounded) accel *= maxAirControl;

        horizVel = Vector3.MoveTowards(horizVel, desiredHoriz, accel * Time.deltaTime);

        // --- Vertical (Y) gravity/jump (optional) ---
        float vy = velocity.y + gravity * Time.deltaTime;

        if (jumpHeight > 0f && isGrounded && Input.GetButtonDown("Jump"))
        {
            vy = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity = new Vector3(horizVel.x, vy, horizVel.z);
    }

    void FixedUpdate()
    {
        // Move using Rigidbody for proper collision
        Vector3 newPos = rb.position + velocity * Time.fixedDeltaTime;

        // Keep feet just above ground if tiny penetration accumulates
        if (isGrounded && velocity.y < 0f) velocity.y = -2f;

        rb.MovePosition(newPos);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        Vector3 p = transform.position + Vector3.up * groundCheckOffset;
        Gizmos.DrawWireSphere(p, groundCheckRadius);
    }
#endif
}
