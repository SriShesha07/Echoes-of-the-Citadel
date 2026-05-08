using TMPro;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    public Camera playerCamera;
    public float interactionRange = 3f;
    public LayerMask interactionMask;

    [Header("UI")]
    public TextMeshProUGUI promptText;

    private Interactable currentInteractable;

    private void Update()
    {
        FindInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private void FindInteractable()
    {
        currentInteractable = null;

        if (playerCamera == null)
        {
            ClearPrompt();
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
        {
            currentInteractable = hit.collider.GetComponentInParent<Interactable>();

            if (currentInteractable != null)
            {
                if (promptText != null)
                {
                    promptText.text = "Press E: " + currentInteractable.prompt;
                }

                return;
            }
        }

        ClearPrompt();
    }

    private void ClearPrompt()
    {
        if (promptText != null)
        {
            promptText.text = "";
        }
    }
}