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
    public GameObject objectiveBackground;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBackground;
    public TextMeshProUGUI tutorialText;
    public GameObject tutorialBackground;

    private Coroutine dialogueRoutine;
    private Coroutine tutorialRoutine;

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
        ApplyHudStyle();
        SetObjective("Find a working fuse to power the elevator.");

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialogueBackground != null)
        {
            dialogueBackground.SetActive(false);
        }

        HideTutorial();
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
            objectiveText.text = "MISSION // " + objective;
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

    public void ShowTutorial(string message, float duration)
    {
        if (tutorialRoutine != null)
        {
            StopCoroutine(tutorialRoutine);
        }

        tutorialRoutine = StartCoroutine(TutorialRoutine(message, duration));
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

    private IEnumerator TutorialRoutine(string message, float duration)
    {
        if (tutorialBackground != null)
        {
            tutorialBackground.SetActive(true);
        }

        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(true);
            tutorialText.text = message;
        }

        yield return new WaitForSeconds(duration);

        HideTutorial();
    }

    private void HideTutorial()
    {
        if (tutorialText != null)
        {
            tutorialText.text = "";
            tutorialText.gameObject.SetActive(false);
        }

        if (tutorialBackground != null)
        {
            tutorialBackground.SetActive(false);
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
        if (uploadStarted || uploadTimer > 0f)
        {
            return;
        }

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

    private void ApplyHudStyle()
    {
        StyleText(objectiveText, new Color(1f, 0.84f, 0.38f, 1f), 27f, FontStyles.Bold);
        StyleText(dialogueText, new Color(0.78f, 0.94f, 1f, 1f), 29f, FontStyles.Bold);
        StyleText(tutorialText, new Color(0.72f, 1f, 0.92f, 1f), 27f, FontStyles.Bold);

        StylePanel(objectiveBackground, new Color(0.035f, 0.07f, 0.09f, 0.82f));
        StylePanel(dialogueBackground, new Color(0.02f, 0.045f, 0.065f, 0.88f));
        StylePanel(tutorialBackground, new Color(0.04f, 0.09f, 0.08f, 0.88f));
    }

    private static void StyleText(TextMeshProUGUI text, Color color, float size, FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        text.color = color;
        text.fontSize = size;
        text.fontStyle = style;
        text.outlineColor = new Color(0f, 0f, 0f, 0.9f);
        text.outlineWidth = 0.16f;
    }

    private static void StylePanel(GameObject panel, Color color)
    {
        if (panel == null)
        {
            return;
        }

        Image image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
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
