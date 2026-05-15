using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 4)]
    public string message;

    public float duration = 5f;
    public bool triggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerMovement>() == null)
        {
            return;
        }

        hasTriggered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowTutorial(message, duration);
        }
    }
}
