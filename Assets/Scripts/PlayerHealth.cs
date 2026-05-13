using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public GameObject deathPanel;

    private bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + Mathf.CeilToInt(currentHealth);
        }
    }

    private void Die()
    {
        isDead = true;

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        // Reset all global state before reloading: timeScale was 0 on death,
        // and cursor was unlocked for the UI. We want a clean slate so the
        // reloaded scene's Start() methods get correct defaults.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // GameManager is a singleton; clear it so the reloaded scene gets a fresh one.
        if (GameManager.Instance != null)
        {
            GameManager.Instance = null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}