using UnityEngine;

/// <summary>
/// Camera controller that smoothly follows the player with optional bounds and smoothing
/// </summary>
public class CameraFollow : MonoBehaviour
{
    #region Public Fields
    [Header("Target")]
    [SerializeField] private Transform player;
    
    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] private float followSpeed = 2f;
    [SerializeField] private bool smoothFollow = true;
    
    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;
    #endregion
    
    #region Private Fields
    private Vector3 velocity = Vector3.zero;
    #endregion
    
    #region Unity Lifecycle
    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: No player assigned and no GameObject with 'Player' tag found!");
            }
        }
    }
    
    void LateUpdate()
    {
        if (player != null)
        {
            FollowPlayer();
        }
    }
    #endregion
    
    #region Camera Methods
    
    /// <summary>
    /// Follow the player with optional smoothing and bounds
    /// </summary>
    private void FollowPlayer()
    {
        Vector3 targetPosition = player.position + offset;
        
        // Apply bounds if enabled
        if (useBounds)
        {
            targetPosition = ApplyBounds(targetPosition);
        }
        
        // Apply movement
        if (smoothFollow)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);
        }
        else
        {
            transform.position = targetPosition;
        }
    }
    
    /// <summary>
    /// Apply camera bounds to target position
    /// </summary>
    /// <param name="targetPos">The target position to constrain</param>
    /// <returns>Constrained position</returns>
    private Vector3 ApplyBounds(Vector3 targetPos)
    {
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        return targetPos;
    }
    
    /// <summary>
    /// Set a new player target
    /// </summary>
    /// <param name="newPlayer">The new player transform to follow</param>
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
    
    /// <summary>
    /// Update camera offset
    /// </summary>
    /// <param name="newOffset">New offset value</param>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    #endregion
}
