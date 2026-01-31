using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform pointA; // Start point
    [SerializeField] private Transform pointB; // End point
    [SerializeField] private float speed = 2f; // Movement speed

    private Vector3 targetPosition;

    private void Start()
    {
        // Start moving towards point B
        targetPosition = pointB.position;
    }

    private void Update()
    {
        // Move the platform towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Switch target when the platform reaches the destination
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = targetPosition == pointA.position ? pointB.position : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Make the player a child of the platform to move with it
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Detach the player from the platform
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}