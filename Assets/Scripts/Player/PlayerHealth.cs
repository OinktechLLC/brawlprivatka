using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Respawn or Game Over logic
        Invoke(nameof(Respawn), 2f);
    }

    void Respawn()
    {
        currentHealth = maxHealth;
        transform.position = Vector3.zero; // Spawn point
    }
}
