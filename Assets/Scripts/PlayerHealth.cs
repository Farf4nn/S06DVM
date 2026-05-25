using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    private int currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    [Header("Player")]
    public MonoBehaviour movementScript;
    private void Start()
    {
        currentHealth = maxHealth;

        UpdateUI();
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void UpdateUI()
    {
        healthText.text = "Vida: " + currentHealth;
    }
    private void Die()
    {
        Debug.Log("Jugador muerto");

        if (movementScript != null)
        {
            movementScript.enabled = false;
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}