using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public Transform pointA;
    public Transform pointB;
    private Transform targetPoint;

    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public LayerMask playerLayer;
    public Transform rayOrigin;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public GameObject hitBox; // Enemy melee attack area
    private float nextAttackTime = 0f;

    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    private Animator anim;
    private Rigidbody2D rb;
    private Transform player;

    private bool isRunning;
    private bool isAttacking;
    private bool isHit;
    private bool isDead;

    private bool facingRight = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        targetPoint = pointB;
    }

    void Update()
    {
        if (isDead) return;

        DetectPlayer();
        UpdateAnimator();

        if (player != null && !isHit)
        {
            // Chase player if detected
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > attackRange)
            {
                MoveTowards(player.position);
            }
            else
            {
                AttackPlayer();
            }
        }
        else if (!isHit)
        {
            // Patrol between A and B when player not detected
            Patrol();
        }
    }

    void DetectPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin.position, transform.right, detectionRange, playerLayer);

        if (hit.collider != null)
        {
            player = hit.collider.transform;
        }
        else
        {
            player = null;
        }

        Debug.DrawRay(rayOrigin.position, transform.right * detectionRange, hit.collider ? Color.red : Color.green);
    }

    void Patrol()
    {
        isRunning = true;

        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            // Switch target point
            targetPoint = targetPoint == pointA ? pointB : pointA;
            Flip();
        }
    }

    void MoveTowards(Vector2 destination)
    {
        isRunning = true;
        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        // Flip direction based on player position
        if ((destination.x > transform.position.x && !facingRight) ||
            (destination.x < transform.position.x && facingRight))
        {
            Flip();
        }
    }

    void AttackPlayer()
    {
        if (Time.time >= nextAttackTime)
        {
            isAttacking = true;
            nextAttackTime = Time.time + attackCooldown;
            StartCoroutine(DoAttack());
        }
    }

    System.Collections.IEnumerator DoAttack()
    {
        hitBox.SetActive(true); // Enable attack hitbox
        yield return new WaitForSeconds(0.3f); // Small delay during attack animation
        hitBox.SetActive(false);
        isAttacking = false;
    }

    void UpdateAnimator()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("isHit", isHit);
        anim.SetBool("isDead", isDead);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        isHit = true;
        UpdateAnimator();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Small delay for hit reaction
            Invoke(nameof(ResetHit), 0.5f);
        }
    }

    void ResetHit()
    {
        isHit = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        UpdateAnimator();
        hitBox.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f);
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When enemy is hit by player's attack collider
        if (collision.CompareTag("PlayerAttack"))
        {
            TakeDamage(1);
        }
    }
}
