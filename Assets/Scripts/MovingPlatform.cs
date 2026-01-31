using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    private Vector3 targetPosition;
    private Transform playerTransform;
    private Rigidbody2D playerRigidbody;

    private void Start()
    {
        targetPosition = pointB.position;
    }

    private void FixedUpdate()
    {
        Vector3 currentPos = transform.position;
        
        // Move platform
        transform.position = Vector3.MoveTowards(currentPos, targetPosition, speed * Time.fixedDeltaTime);
        
        // Calculate how much the platform moved this frame
        Vector3 deltaMovement = transform.position - currentPos;

        // Move player with the platform using Rigidbody2D
        if (playerRigidbody != null)
        {
            playerRigidbody.MovePosition(playerRigidbody.position + (Vector2)deltaMovement);
        }

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
            playerTransform = collision.transform;
            playerRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerTransform = null;
            playerRigidbody = null;
        }
    }
}