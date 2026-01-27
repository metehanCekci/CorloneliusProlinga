using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float gravityStrength = 5f;
    [SerializeField] private float groundRayDistance = 0.1f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float maxJumpHeight = 5f;

    [SerializeField] private float wallRayDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    // --- YENİ: DUVARLAR İÇİN AYRI KATMAN ---
    [SerializeField] private LayerMask wallLayer;

    private Vector3 velocity = Vector3.zero;
    private bool isGrounded = false;
    private bool isJumping = false;
    private bool isTouchingWall = false;
    private float jumpStartHeight = 0f;
    private Collider2D col;

    void Start()
    {
        col = GetComponent<Collider2D>();

        // Default atamalar (Inspector'dan seçmeyi unutma!)
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");
        if (wallLayer == 0) wallLayer = LayerMask.GetMask("Wall");
    }

    void Update()
    {
        CheckGround();

        if (isJumping)
        {
            velocity.y = jumpForce;
            float currentHeight = transform.position.y - jumpStartHeight;
            if (currentHeight >= maxJumpHeight)
            {
                isJumping = false;
            }
        }
        else
        {
            if (isGrounded && velocity.y <= 0)
            {
                velocity.y = 0;
            }
            else
            {
                velocity.y -= gravityStrength * Time.deltaTime;
            }
        }

        CheckWalls();

        transform.position += velocity * Time.deltaTime;
    }

    private void CheckGround()
    {
        Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundRayDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private void CheckWalls()
    {
        isTouchingWall = false; // Her karede önce sıfırla

        if (velocity.x != 0)
        {
            float directionX = Mathf.Sign(velocity.x);
            Vector2 rayOrigin = new Vector2(directionX > 0 ? col.bounds.max.x : col.bounds.min.x, transform.position.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, wallRayDistance, wallLayer);

            if (hit.collider != null)
            {
                velocity.x = 0;
                isTouchingWall = true; // Duvara değdiğimizi kaydet
            }
        }
    }

    public void StartJump() { isJumping = true; jumpStartHeight = transform.position.y; }
    public void EndJump() { isJumping = false; }
    public Vector3 GetVelocity() { return velocity; }
    public void SetVelocity(Vector3 newVelocity) { velocity = newVelocity; }
    public bool IsGrounded() { return isGrounded; }
    public bool IsTouchingWall() { return isTouchingWall; }
}
