using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    

    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBackground;

    private Coroutine dialogueRoutine;

    [Header("Level 1 Progress")]
    public bool hasFuse;
    public bool fuseInstalled;

    [Header("Level 1 Door")]
    public DoorController level1ExitDoor;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetObjective("Find a working fuse to power the elevator.");

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(false);
        }
    }

    public void SetObjective(string objective)
    {
        if (objectiveText != null)
        {
            objectiveText.text = "Objective: " + objective;
        }
    }

    public void ShowDialogue(string message, float duration)
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(DialogueRoutine(message, duration));
    }

    private IEnumerator DialogueRoutine(string message, float duration)
    {
        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.text = message;
        }

        yield return new WaitForSeconds(duration);

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(false);
        }
    }

    public void CollectFuse()
    {
        hasFuse = true;
        SetObjective("Install the fuse in the fuse box.");
        ShowDialogue("KAEL: Fuse is intact. Finally, something in this place still works.", 4f);
    }

    public void InstallFuse()
    {
        if (!hasFuse)
        {
            ShowDialogue("The fuse box is dead. You need a working fuse.", 3f);
            return;
        }

        fuseInstalled = true;
        SetObjective("Elevator power restored. Reach the exit door.");
        ShowDialogue("WARDEN: Power rerouted. Containment breach escalating.", 4f);
    }

    public void UseLevel1ExitDoor()
    {
        if (!fuseInstalled)
        {
            ShowDialogue("The elevator has no power.", 3f);
            return;
        }

        if (level1ExitDoor != null)
        {
            level1ExitDoor.OpenDoor();
        }

        SetObjective("Enter the Silent Halls.");
        ShowDialogue("WARDEN: Unauthorized entity has entered Research Sector A.", 4f);
    }

    public void ReadWorkerDatapad()
    {
        ShowDialogue("DATAPAD: They locked the doors. Why won't they open the doors?", 6f);
    }
}