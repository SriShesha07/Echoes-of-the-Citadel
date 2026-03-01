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
    private float normalHeight;
    public float crouchHeight = 0.5f;

    Vector3 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        normalHeight = transform.localScale.y;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        movement = (transform.right * x + transform.forward * z).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, normalHeight, transform.localScale.z);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed; 
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed; 
        }

        Vector3 newVelocity = movement * currentSpeed;
        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
    }
}