using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Movement")]
    public Vector3 openOffset = new Vector3(0f, 4f, 0f);
    public float openSpeed = 2f;

    [Header("State")]
    public bool isOpen;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    private void Update()
    {
        Vector3 targetPosition = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * openSpeed);
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }
}