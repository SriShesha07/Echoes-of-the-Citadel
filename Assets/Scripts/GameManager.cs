using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Level 2 Progress")]
    public bool hasArisHandprint;

    [Header("Level 2 Door")]
    public DoorController dnaDoor;

   

    [Header("Ending UI")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingTitleText;
    public TextMeshProUGUI endingBodyText;

    [Header("Upload UI")]
    public GameObject uploadPanel;
    public Slider uploadSlider;
    public TextMeshProUGUI uploadPercentText;

    [Header("Upload Settings")]
    public float uploadDuration = 45f;

    private bool uploadStarted;
    private float uploadTimer;

    [Header("Final Wave")]
    public EnemySpawner finalWaveSpawner;

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

    private void Update()
    {
        if (uploadStarted)
        {
            UpdateUpload();
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

    public void CollectArisHandprint()
    {
        hasArisHandprint = true;
        SetObjective("Use Dr. Aris's handprint to open the DNA-locked door.");
        ShowDialogue("KAEL: Sorry, Doctor. I need your clearance.", 4f);
    }

    public void UseDnaDoor()
    {
        if (!hasArisHandprint)
        {
            ShowDialogue("DNA lock rejected. Dr. Aris clearance required.", 3f);
            return;
        }

        if (dnaDoor != null)
        {
            dnaDoor.OpenDoor();
        }

        SetObjective("Proceed deeper into the Citadel.");
        ShowDialogue("DNA clearance accepted. Welcome, Dr. Aris.", 4f);
    }

    public void StartMercenaryEnding()
    {
        ShowEnding(
            "Ending A: Mercenary",
            "Kael extracted the Aether Core and delivered it to the client. The credits cleared. Earth did not."
        );
    }

    public void StartHeroPath()
    {
        uploadStarted = true;
        uploadTimer = 0f;

        if (uploadPanel != null)
        {
            uploadPanel.SetActive(true);
        }

        if (uploadSlider != null)
        {
            uploadSlider.value = 0f;
        }

        if (uploadPercentText != null)
        {
            uploadPercentText.text = "0%";
        }

        SetObjective("Survive until the Aether data upload reaches 100%.");
        ShowDialogue("WARDEN: Theft of planetary future detected. Lethal security response authorized.", 5f);

        if (finalWaveSpawner != null)
        {
            finalWaveSpawner.StartSpawning();
        }
    }

    private void ShowEnding(string title, string body)
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }

        if (endingTitleText != null)
        {
            endingTitleText.text = title;
        }

        if (endingBodyText != null)
        {
            endingBodyText.text = body;
        }
    }

    private void UpdateUpload()
    {
        uploadTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(uploadTimer / uploadDuration);

        if (uploadSlider != null)
        {
            uploadSlider.value = progress;
        }

        if (uploadPercentText != null)
        {
            uploadPercentText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }

        if (progress >= 1f)
        {
            uploadStarted = false;
            ShowHeroEnding();
        }
    }

    private void ShowHeroEnding()
    {
        ShowEnding(
            "Ending B: Hero",
            "The Aether data reached the public network. For the first time in decades, Earth had a chance."
        );
    }

    // Hookable from the ending panel button as well as any other "back to start" UI.
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Instance = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}