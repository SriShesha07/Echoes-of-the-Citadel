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

    [Tooltip("Prompt text color. Cyan/yellow read well on most backgrounds.")]
    public Color promptColor = new Color(1f, 0.92f, 0.25f, 1f); // bright gold

    [Tooltip("Outline color behind prompt text — keeps it readable on bright/dark surfaces.")]
    public Color promptOutlineColor = new Color(0f, 0f, 0f, 1f);

    [Range(0f, 1f)]
    public float promptOutlineWidth = 0.25f;

    [Tooltip("Bold the prompt for better legibility.")]
    public bool promptBold = true;

    private Interactable currentInteractable;

    private void Start()
    {
        if (promptText != null)
        {
            ApplyPromptStyle();
            promptText.text = "";
        }
    }

    private void ApplyPromptStyle()
    {
        promptText.color = promptColor;
        if (promptBold)
        {
            promptText.fontStyle |= FontStyles.Bold;
        }
        promptText.outlineColor = promptOutlineColor;
        promptText.outlineWidth = promptOutlineWidth;
        // Make sure the text renders on top of slight overdraw from glow.
        promptText.enableWordWrapping = false;
    }

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
        Interactable found = null;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
            {
                found = hit.collider.GetComponentInParent<Interactable>();
            }
        }

        // Highlight transitions: turn off the old one, turn on the new one.
        if (found != currentInteractable)
        {
            if (currentInteractable != null)
            {
                currentInteractable.SetHighlight(false);
            }
            if (found != null)
            {
                found.SetHighlight(true);
            }
            currentInteractable = found;
        }

        if (currentInteractable != null)
        {
            if (promptText != null)
            {
                promptText.color = promptColor;
                promptText.text = "[E] " + currentInteractable.prompt;
            }
        }
        else
        {
            ClearPrompt();
        }
    }

    private void ClearPrompt()
    {
        if (promptText != null)
        {
            promptText.text = "";
        }
    }

    private void OnDisable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.SetHighlight(false);
            currentInteractable = null;
        }
    }
}
