using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Gravity : MonoBehaviour
{
    [Header("Zıplama ve Fizik")]
    [SerializeField] private float gravityStrength = 35f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float maxJumpHeight = 3f;

    [Header("Zemin ve Rampa Ayarları")]
    [SerializeField] private float groundCheckDist = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float sideOffset = 0.3f; // Ön ve arka teker mesafesi
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Çarpışma Katmanları")]
    [SerializeField] private LayerMask railLayer;
    [SerializeField] private LayerMask wallLayer;

    private Rigidbody2D rb;
    private ControllerScript controller;
    private Collider2D col;
    private Vector2 velocity = Vector2.zero;
    private bool isGrounded;
    private bool isJumping;
    private float jumpStartHeight;
    private float groundCooldown;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        controller = GetComponent<ControllerScript>();

        // Katmanları kontrol et, atanmamışsa isimden bul
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");
        if (railLayer == 0) railLayer = LayerMask.GetMask("Rail");
        if (wallLayer == 0) wallLayer = LayerMask.GetMask("Wall", "Ground");

        // Rigidbody Ayarlarını Sabitle
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        // Grind veya Wallride yaparken yerçekimini ve zemin kontrolünü devre dışı bırak
        if (controller != null && (controller.isGrinding || controller.IsOnWall()))
        {
            isGrounded = false;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.fixedDeltaTime * rotationSpeed);
            return;
        }

        // Zıplama Cooldown Kontrolü
        if (groundCooldown > 0)
        {
            groundCooldown -= Time.fixedDeltaTime;
            isGrounded = false;
        }
        else
        {
            PerformGroundCheck();
        }

        HandleVerticalMovement();

        // --- DUVAR ÇARPIŞMA KONTROLÜ ---
        CheckWallCollision();
        CheckRailCollision();
        // ------------------------------

        // MovePosition kullanarak pozisyonu uygula (Fizik motoruyla çakışmaz)
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void CheckWallCollision()
    {
        // Hareket etmiyorsak kontrol etmeye gerek yok
        if (Mathf.Approximately(velocity.x, 0f)) return;

        float dir = Mathf.Sign(velocity.x);
        
        // Karakterin merkezinden gideceği yöne doğru raycast
        // col.bounds.extents.x + 0.1f mesafesi karakterin tam kenarından az ötesini kontrol eder
        Vector2 origin = col.bounds.center;
        float checkDist = col.bounds.extents.x + 0.1f;
        
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, checkDist, wallLayer);

        // Debug Çizgisi (Duvar kontrolünü görmen için)
        Debug.DrawRay(origin, Vector2.right * dir * checkDist, hit.collider != null ? Color.blue : Color.yellow);

        if (hit.collider != null)
        {
            velocity.x = 0; // Duvara çarptığı an yatay hızı sıfırla
        }
    }

    private void PerformGroundCheck()
    {
        // Karakterin rotasyonuna göre ray başlangıç noktalarını ayarla
        Vector2 leftOrigin = rb.position + (Vector2)(transform.right * -sideOffset) + (Vector2)(transform.up * 0.1f);
        Vector2 rightOrigin = rb.position + (Vector2)(transform.right * sideOffset) + (Vector2)(transform.up * 0.1f);

        RaycastHit2D hitL = Physics2D.Raycast(leftOrigin, -transform.up, groundCheckDist, groundLayer);
        RaycastHit2D hitR = Physics2D.Raycast(rightOrigin, -transform.up, groundCheckDist, groundLayer);

        // Debug Çizgileri
        Debug.DrawRay(leftOrigin, -transform.up * groundCheckDist, hitL ? Color.green : Color.red);
        Debug.DrawRay(rightOrigin, -transform.up * groundCheckDist, hitR ? Color.green : Color.red);

        RaycastHit2D hit = hitL ? hitL : hitR;

        if (hit.collider != null && velocity.y <= 0.1f)
        {
            isGrounded = true;
            velocity.y = 0;

            // --- YERE YAPIŞMA VE SNAP SİSTEMİ ---
            float groundY = hit.point.y;
            float currentFeetY = col.bounds.min.y;
            float diff = groundY - currentFeetY;

            // Milimetrik farkları düzelterek yere yapıştır
            if (Mathf.Abs(diff) > 0.01f)
            {
                rb.position = new Vector2(rb.position.x, rb.position.y + diff);
            }

            if (controller != null) controller.ResetDash();
        }
        else
        {
            isGrounded = false;
            // Havadayken dikleşme
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.fixedDeltaTime * (rotationSpeed * 0.5f));
        }
    }

    private void HandleVerticalMovement()
    {
        if (isJumping)
        {
            velocity.y = jumpForce;
            if (rb.position.y - jumpStartHeight >= maxJumpHeight) isJumping = false;
        }
        else if (!isGrounded)
        {
            velocity.y -= gravityStrength * Time.fixedDeltaTime;
        }
    }

    private void CheckRailCollision()
    {
        if (controller == null || !controller.CanEnterRail()) return;
        
        Collider2D hitRail = Physics2D.OverlapBox(col.bounds.center, col.bounds.size, 0f, railLayer);
        if (hitRail != null)
        {
            var rail = hitRail.GetComponent<RailSystem>();
            if (rail != null) rail.StartGrindFromManual(controller, Mathf.Abs(velocity.x), velocity.x);
        }
    }

    // --- Public Metotlar ---
    public void StartJump() 
    { 
        isJumping = true; 
        jumpStartHeight = rb.position.y; 
        groundCooldown = 0.2f; 
        isGrounded = false;
    }

    public void EndJump() => isJumping = false;

    public void SetVelocity(Vector2 newVel) 
    { 
        velocity = newVel; 
        if (newVel.y > 0) groundCooldown = 0.2f; 
    }

    public Vector2 GetVelocity() => velocity;
    public bool IsGrounded() => isGrounded;
}
