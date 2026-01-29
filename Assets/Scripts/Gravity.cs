using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float gravityStrength = 30f;
    [SerializeField] private float groundRayDistance = 0.1f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float maxJumpHeight = 2.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask railLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.05f;

    private Vector3 velocity = Vector3.zero;
    private bool isGrounded = false;
    private bool isJumping = false;
    private float jumpStartHeight = 0f;
    private float groundCheckCooldown = 0f; // Rail'den çıktıktan sonra kısa süre ground check yapılmaz
    private Collider2D col;
    private ControllerScript controller;

    void Start()
    {
        col = GetComponent<Collider2D>();
        controller = GetComponent<ControllerScript>();
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");
        if (railLayer == 0) railLayer = LayerMask.GetMask("Rail");
        if (wallLayer == 0) wallLayer = LayerMask.GetMask("Wall", "Ground");
    }

    void Update()
    {
        if (controller != null && controller.isGrinding) return;
        if (controller != null && controller.IsOnWall()) return; // Duvardayken gravity işleme

        // Ground check cooldown
        if (groundCheckCooldown > 0)
        {
            groundCheckCooldown -= Time.deltaTime;
            isGrounded = false; // Cooldown süresince yerde değiliz
        }
        else
        {
            CheckGround();
        }
        
        CheckRailCollision();

        if (isJumping)
        {
            velocity.y = jumpForce;
            if (transform.position.y - jumpStartHeight >= maxJumpHeight) isJumping = false;
        }
        else
        {
            if (isGrounded && velocity.y <= 0) velocity.y = 0;
            else velocity.y -= gravityStrength * Time.deltaTime;
        }

        // Wall collision check - yatay hareket için
        CheckWallCollision();

        transform.position += velocity * Time.deltaTime;
    }

    private void CheckWallCollision()
    {
        if (col == null || Mathf.Approximately(velocity.x, 0f)) return;

        float dir = Mathf.Sign(velocity.x);
        Vector2 origin = new Vector2(
            dir > 0 ? col.bounds.max.x : col.bounds.min.x,
            col.bounds.center.y
        );
        
        float checkDist = Mathf.Abs(velocity.x * Time.deltaTime) + wallCheckDistance;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, checkDist, wallLayer);
        
        if (hit.collider != null)
        {
            // Duvara çarptık, X velocity'yi sıfırla
            velocity.x = 0;
        }
    }

    private void CheckRailCollision()
    {
        if (controller == null || !controller.CanEnterRail()) return;
        
        Collider2D hitRail = Physics2D.OverlapBox(col.bounds.center, col.bounds.size, 0f, railLayer);
        if (hitRail != null)
        {
            RailSystem rail = hitRail.GetComponent<RailSystem>();
            if (rail != null)
            {
                // Karakterin mevcut hızını ve yönünü raya aktar
                float entrySpeed = Mathf.Abs(velocity.x);
                float entryDirection = velocity.x; // Pozitif = sağ, negatif = sol
                rail.StartGrindFromManual(controller, entrySpeed, entryDirection);
            }
        }
    }

    private void CheckGround()
    {
        if (col == null)
        {
            Debug.LogError("COLLIDER YOK!");
            return;
        }
        
        Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundRayDistance, groundLayer);
        
        // Debug ray çiz
        Debug.DrawRay(rayOrigin, Vector2.down * groundRayDistance, hit.collider != null ? Color.green : Color.red);
        
        if (hit.collider != null)
        {
            isGrounded = true;
            
            // Zemine gömülmeyi önle - karakteri zeminin üstüne oturt
            if (velocity.y <= 0)
            {
                float groundY = hit.point.y;
                float feetOffset = col.bounds.min.y - transform.position.y;
                float targetY = groundY - feetOffset + 0.01f; // Küçük offset
                
                if (transform.position.y < targetY)
                {
                    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                }
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    public void StartJump() { isJumping = true; jumpStartHeight = transform.position.y; }
    // Call when jump button is released to stop upward hold
    public void EndJump() { isJumping = false; }
    public void SetVelocity(Vector3 newVel) 
    { 
        velocity = newVel;
        // Yukarı hız verildiyse kısa süre ground check'i devre dışı bırak
        if (newVel.y > 0) groundCheckCooldown = 0.15f;
    }
    public Vector3 GetVelocity() => velocity;
    public bool IsGrounded() => isGrounded;
    
    // Wall climb için - duvardayken pozisyon güncellemesi
    public void ApplyWallMovement()
    {
        transform.position += velocity * Time.deltaTime;
    }
}