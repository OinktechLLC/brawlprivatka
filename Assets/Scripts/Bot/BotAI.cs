using UnityEngine;
using UnityEngine.AI;

public class BotAI : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;
    public float speed = 3.5f;
    public float damage = 5f;
    public float fireRate = 1f;
    public float range = 50f;
    
    private NavMeshAgent agent;
    private Transform player;
    private float nextFireTime = 0f;
    private bool isDead = false;
    private LineRenderer lineRenderer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Simple visual representation
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= range)
        {
            // Stop and shoot
            agent.isStopped = true;
            LookAtPlayer();
            
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // Chase player
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void Shoot()
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position + Vector3.up * 1.5f);
        lineRenderer.SetPosition(1, player.position + Vector3.up * 1.5f);
        
        Invoke(nameof(HideLaser), 0.1f);

        // Deal damage to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    void HideLaser()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true;
        
        // Drop cookies
        for (int i = 0; i < 3; i++)
        {
            CookieSpawner.SpawnCookie(transform.position + Random.insideUnitSphere * 2f);
        }
        
        // Disable after delay
        Invoke(nameof(DisableBot), 2f);
    }

    void DisableBot()
    {
        gameObject.SetActive(false);
    }
}
