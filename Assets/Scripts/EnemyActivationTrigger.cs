using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    [Header("Enemies To Activate")]
    public EnemyAI[] enemiesToActivate;

    [Header("Settings")]
    public bool triggerOnce = true;
    public bool hideEnemiesUntilTriggered = false;

    [Header("Optional Dialogue")]
    [TextArea(2, 4)]
    public string activationMessage = "WARDEN: Security droids deployed.";
    public float messageDuration = 4f;

    private bool hasTriggered;

    private void Awake()
    {
        if (!hideEnemiesUntilTriggered)
        {
            return;
        }

        foreach (EnemyAI enemy in enemiesToActivate)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

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

        ActivateEnemies();

        if (GameManager.Instance != null && !string.IsNullOrEmpty(activationMessage))
        {
            GameManager.Instance.ShowDialogue(activationMessage, messageDuration);
        }
    }

    private void ActivateEnemies()
    {
        foreach (EnemyAI enemy in enemiesToActivate)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(true);
                enemy.enabled = true;

                EnemyAttack enemyAttack = enemy.GetComponent<EnemyAttack>();

                if (enemyAttack != null)
                {
                    enemyAttack.enabled = true;
                }
            }
        }
    }
}
