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
    [SerializeField] private float slopeSnapForce = 50f; // Slope'a yapışma gücü
    [SerializeField] private float maxSlopeAngle = 60f; // Maksimum tırmanılabilir eğim

    private Vector3 velocity = Vector3.zero;
    private bool isGrounded = false;
    private bool isJumping = false;
    private bool isOnSlope = false;
    private Vector2 groundNormal = Vector2.up;
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
            if (isGrounded && velocity.y <= 0)
            {
                // Slope üzerindeyken gravity yerine zemine yapıştır
                if (isOnSlope && !isJumping)
                {
                    // Slope'a doğru hafif bir kuvvet uygula (zemine yapışsın)
                    velocity.y = -slopeSnapForce * Time.deltaTime;
                }
                else
                {
                    velocity.y = 0;
                }
            }
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
        
        // Daha uzun ray kullan - slope için daha iyi algılama
        float slopeRayDistance = groundRayDistance + 0.2f;
        Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, slopeRayDistance, groundLayer);
        
        // Debug ray çiz
        Debug.DrawRay(rayOrigin, Vector2.down * slopeRayDistance, hit.collider != null ? Color.green : Color.red);
        
        if (hit.collider != null && hit.distance <= groundRayDistance + 0.05f)
        {
            isGrounded = true;
            groundNormal = hit.normal;
            
            // Slope algılama - normal vektörü yukarı değilse slope üzerindeyiz
            float slopeAngle = Vector2.Angle(Vector2.up, groundNormal);
            isOnSlope = slopeAngle > 1f && slopeAngle <= maxSlopeAngle;
            
            // Debug - slope göster
            if (isOnSlope)
            {
                Debug.DrawRay(hit.point, groundNormal * 0.5f, Color.yellow);
            }
            
            // Zemine gömülmeyi önle - karakteri zeminin üstüne oturt
            if (velocity.y <= 0)
            {
                float groundY = hit.point.y;
                float feetOffset = col.bounds.min.y - transform.position.y;
                float targetY = groundY - feetOffset + 0.01f; // Küçük offset
                
                // Slope üzerindeyken sürekli zemine snap yap
                if (isOnSlope || transform.position.y < targetY)
                {
                    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                }
            }
        }
        else if (hit.collider != null && hit.distance <= slopeRayDistance)
        {
            // Zemine yakınız ama tam değmiyoruz - slope'tan aşağı kayarken
            isOnSlope = true;
            isGrounded = false;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            isOnSlope = false;
            groundNormal = Vector2.up;
        }
    }

    public void StartJump() { isJumping = true; isOnSlope = false; jumpStartHeight = transform.position.y; }
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
    public bool IsOnSlope() => isOnSlope;
    public Vector2 GetGroundNormal() => groundNormal;
    
    // Wall climb için - duvardayken pozisyon güncellemesi
    public void ApplyWallMovement()
    {
        transform.position += velocity * Time.deltaTime;
    }
}