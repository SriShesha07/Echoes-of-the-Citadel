using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string message;

    public float duration = 4f;
    public bool triggerOnce = true;

    [Header("Optional Objective Update")]
    public bool updateObjective;
    [TextArea(1, 3)]
    public string newObjective;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        hasTriggered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowDialogue(message, duration);

            if (updateObjective)
            {
                GameManager.Instance.SetObjective(newObjective);
            }
        }
    }
}