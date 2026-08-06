
using UnityEngine;
using System.Collections;

/// <summary>
/// Handles player death and respawn mechanics
/// </summary>
public class PlayerDeath : MonoBehaviour
{
    #region Public Fields
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private string deathZoneTag = "DeathZone";
    
    [Header("Death Effects")]
    [SerializeField] private bool disableMovementOnDeath = true;
    [SerializeField] private bool hideSpriteOnDeath = true;
    [SerializeField] private AudioClip deathSound;
    #endregion
    
    #region Private Fields
    private bool isDead = false;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;
    private AudioSource audioSource;
    private Vector2 originalVelocity;
    #endregion
    
    #region Unity Lifecycle
    void Start()
    {
        InitializeComponents();
        ValidateRespawnPoint();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(deathZoneTag) && !isDead)
        {
            StartCoroutine(DieAndRespawn());
        }
    }
    #endregion
    
    #region Initialization
    
    /// <summary>
    /// Initialize all required components
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
        
        if (rb == null) Debug.LogError("PlayerDeath: Rigidbody2D component not found!");
        if (spriteRenderer == null) Debug.LogError("PlayerDeath: SpriteRenderer component not found!");
    }
    
    /// <summary>
    /// Validate that respawn point is assigned
    /// </summary>
    private void ValidateRespawnPoint()
    {
        if (respawnPoint == null)
        {
            Debug.LogError("PlayerDeath: Respawn point is not assigned!");
        }
    }
    
    #endregion
    
    #region Death and Respawn
    
    /// <summary>
    /// Handle player death and respawn sequence
    /// </summary>
    private IEnumerator DieAndRespawn()
    {
        isDead = true;
        
        // Store original velocity for potential restoration
        if (rb != null)
        {
            originalVelocity = rb.linearVelocity;
        }
        
        // Apply death effects
        ApplyDeathEffects();
        
        // Play death sound if available
        PlayDeathSound();
        
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Respawn player
        RespawnPlayer();
        
        isDead = false;
    }
    
    /// <summary>
    /// Apply visual and movement effects when player dies
    /// </summary>
    private void ApplyDeathEffects()
    {
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // Disable player movement script
        if (disableMovementOnDeath && playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // Hide sprite
        if (hideSpriteOnDeath && spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }
    
    /// <summary>
    /// Respawn the player at the respawn point
    /// </summary>
    private void RespawnPlayer()
    {
        if (respawnPoint == null)
        {
            Debug.LogError("PlayerDeath: Cannot respawn - no respawn point assigned!");
            return;
        }
        
        // Move player to respawn point
        transform.position = respawnPoint.position;
        
        // Restore physics
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
        
        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        // Show sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
    
    /// <summary>
    /// Play death sound effect
    /// </summary>
    private void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Set a new respawn point
    /// </summary>
    /// <param name="newRespawnPoint">The new respawn point transform</param>
    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
    }
    
    /// <summary>
    /// Check if player is currently dead
    /// </summary>
    /// <returns>True if player is dead</returns>
    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// Force respawn the player (useful for debug or external triggers)
    /// </summary>
    public void ForceRespawn()
    {
        if (isDead)
        {
            StopAllCoroutines();
            RespawnPlayer();
            isDead = false;
        }
    }
    
    #endregion
}
