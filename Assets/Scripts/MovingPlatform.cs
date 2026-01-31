using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    private Vector3 targetPosition;
    private Gravity playerGravity;
    private Vector3 lastPlatformPosition;

    private void Start()
    {
        targetPosition = pointB.position;
        lastPlatformPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 currentPos = transform.position;
        
        // Move platform
        transform.position = Vector3.MoveTowards(currentPos, targetPosition, speed * Time.fixedDeltaTime);
        
        // Calculate how much the platform moved this frame
        Vector3 platformMovement = transform.position - lastPlatformPosition;

        // If player is on platform, add platform movement to player's velocity
        if (playerGravity != null)
        {
            Vector2 currentVelocity = playerGravity.GetVelocity();
            Vector2 newVelocity = currentVelocity + (Vector2)platformMovement / Time.fixedDeltaTime;
            playerGravity.SetVelocity(newVelocity);
        }

        lastPlatformPosition = transform.position;

        // Switch direction when reaching target
        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            targetPosition = targetPosition == pointA.position ? pointB.position : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerGravity = collision.gameObject.GetComponent<Gravity>();
            if (playerGravity != null)
            {
                lastPlatformPosition = transform.position;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerGravity == null)
        {
            playerGravity = collision.gameObject.GetComponent<Gravity>();
            lastPlatformPosition = transform.position;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerGravity = null;
        }
    }
}
