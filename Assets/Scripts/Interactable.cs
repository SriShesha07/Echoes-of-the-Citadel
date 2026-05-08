using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Prompt")]
    public string prompt = "Interact";

    [Header("Settings")]
    public bool disableAfterInteract = false;

    [Header("Events")]
    public UnityEvent onInteract;

    private bool hasInteracted;

    public void Interact()
    {
        if (hasInteracted)
        {
            return;
        }

        onInteract.Invoke();

        if (disableAfterInteract)
        {
            hasInteracted = true;
            gameObject.SetActive(false);
        }
    }
}