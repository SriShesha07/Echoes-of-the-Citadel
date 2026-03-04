using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 3f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;

    [Header("Body")]
    public Rigidbody rb;
    public CapsuleCollider playerCollider;
    private float normalHeight;
    private Vector3 normalCenter;
    public float crouchHeight = 0.5f;

    [Header("Animation")]
    public Animator animator;

    Vector3 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        normalHeight = playerCollider.height;
        normalCenter = playerCollider.center;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        movement = (transform.right * x + transform.forward * z).normalized;

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, (normalHeight / 2f) + 0.2f);

        float animationSpeed = 0f;
        if (movement.magnitude > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift)) animationSpeed = sprintSpeed;
            else if (Input.GetKey(KeyCode.LeftControl)) animationSpeed = crouchSpeed;
            else animationSpeed = walkSpeed;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", animationSpeed);
            animator.SetBool("IsGrounded", isGrounded);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            playerCollider.height = crouchHeight;
            playerCollider.center = new Vector3(normalCenter.x, normalCenter.y - (normalHeight - crouchHeight) / 2f, normalCenter.z);
            if (animator != null) animator.SetBool("IsCrouching", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            playerCollider.height = normalHeight;
            playerCollider.center = normalCenter;
            if (animator != null) animator.SetBool("IsCrouching", false);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftControl)) currentSpeed = crouchSpeed;
        else if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = sprintSpeed;

        Vector3 newVelocity = movement * currentSpeed;
        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
    }
}