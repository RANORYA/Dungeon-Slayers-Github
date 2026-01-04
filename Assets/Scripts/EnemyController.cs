using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using StarterAssets; // ThirdPersonController'a ulaşmak için

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;
    public float damage = 10f;
    public float attackRange = 1.5f;

    [Header("AI Settings")]
    public float detectionRange = 10f;

    [Header("Loot Settings")]
    public GameObject lootPrefab;
    
    // Referanslar
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private bool isDead = false;
    public Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Oyuncuyu bul (Tag'in "Player" olduğundan emin ol)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Yapay zeka döngüsünü başlat
        StartCoroutine(ThinkRoutine());
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    IEnumerator ThinkRoutine()
    {
        while (!isDead && player != null)
        {
            var playerScript = player.GetComponent<ThirdPersonController>();
            if (playerScript != null && playerScript.isDead)
            {
                // Oyuncu öldüyse her şeyi durdur ve döngüden çık
                agent.isStopped = true;
                animator.SetBool("IsAttacking", false);
                animator.SetFloat("Speed", 0f);
                yield break; // Coroutine'i tamamen sonlandırır
            }

            float distance = Vector3.Distance(transform.position, player.position);
            
            // 1. Oyuncu menzilde mi?
            if (distance <= detectionRange)
            {
                // 2. Saldırı mesafesinde mi?
                if (distance <= attackRange)
                {
                    // Dur ve Saldır
                    agent.isStopped = true;
                    animator.SetBool("IsAttacking", true);
                    animator.SetFloat("Speed", 0f);
                    
                    // Yüzünü oyuncuya dön
                    RotateTowardsPlayer();
                }
                else
                {
                    // Kovala
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                    animator.SetBool("IsAttacking", false);
                    animator.SetFloat("Speed", agent.velocity.magnitude);
                }
            }
            else
            {
                // Oyuncu uzaktaysa bekle (Idle)
                agent.isStopped = true;
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsAttacking", false);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void HitPlayer()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        
        // Eğer hala menzildeyse hasar ver
        if (distance <= attackRange + 0.5f) 
        {
            var playerScript = player.GetComponent<ThirdPersonController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        if (currentHealth <= 0)
        {
            // Ölünce can barını gizle
            if(healthSlider != null) healthSlider.gameObject.SetActive(false);
            Die();
        }
        animator.SetTrigger("Hit"); // Hasar alma animasyonu varsa
        Debug.Log("Düşman Canı: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return; // Çift tetiklenmeyi önle
        isDead = true;

        // 1. Takibi ve Hareketi Durdur
        agent.isStopped = true;
        agent.enabled = false; // Agent'ı tamamen kapat ki itilmesin

        // 2. Ölüm Animasyonunu Tetikle
        if (animator != null) 
        {
            animator.SetBool("IsAttacking", false); // Saldırıyorsa durdur
            animator.SetTrigger("Die"); 
        }

        // 3. Fiziksel Engeli Kaldır
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 4. Ganimet (Loot) Oluştur
        if (lootPrefab != null)
        {
            // Sandığı düşmanın ayaklarının dibine, biraz yukarıda oluştur
            Vector3 spawnPos = transform.position + Vector3.up * 0.2f;
            Instantiate(lootPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log("Canavar Öldü ve Ganimet Bıraktı!");
        
        // 5. Objeyi sahneden temizle (Örn: 10 saniye sonra)
        Destroy(gameObject, 10f);
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}