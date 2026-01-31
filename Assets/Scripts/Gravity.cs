using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Gravity : MonoBehaviour
{
    [Header("Zıplama ve Fizik")]
    [SerializeField] private float gravityStrength = 35f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float maxJumpHeight = 3f;

    [Header("Zemin, Tavan ve Rampa")]
    [SerializeField] private float groundCheckDist = 0.4f;
    [SerializeField] private float ceilingCheckDist = 0.4f; // Tavan kontrol mesafesi
    [SerializeField] private LayerMask groundLayer;
    
    // YENİ: Sadece tavanı algılayacak layer
    [SerializeField] private LayerMask ceilingLayer; 
    
    [SerializeField] private float sideOffset = 0.3f;
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

        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");
        if (railLayer == 0) railLayer = LayerMask.GetMask("Rail");
        if (wallLayer == 0) wallLayer = LayerMask.GetMask("Wall");
        
        // Eğer Tavan layerı seçilmediyse otomatik olarak Ground veya Wall yapalım ki kod bozulmasın
        if (ceilingLayer == 0) ceilingLayer = groundLayer; 

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        if (controller != null && (controller.isGrinding || controller.IsOnWall()))
        {
            isGrounded = false;
            return;
        }

        // --- ZEMİN KONTROLÜ ---
        if (groundCooldown > 0)
        {
            groundCooldown -= Time.fixedDeltaTime;
            isGrounded = false;
        }
        else
        {
            
            // Fiziği her zaman dik tutuyoruz (Görseli SlopeStabilizer hallediyor)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.fixedDeltaTime * rotationSpeed);
        }
        PerformGroundCheck();

        HandleVerticalMovement();

        // --- TAVAN KONTROLÜ ---
        CheckCeilingCollision(); 

        // --- DİĞER KONTROLLER ---
        CheckWallCollision();
        CheckRailCollision();

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    // --- GÜNCELLENMİŞ TAVAN KONTROLÜ ---
    private void CheckCeilingCollision()
    {
        // Sadece yukarı çıkarken kontrol et
        if (velocity.y <= 0) return;

        // Kafanın üstünden iki ışın (Sağ ve Sol köşe)
        // sideOffset kullanarak karakterin genişliği kadar açıyoruz ama biraz içeriden atıyoruz
        // (Böylece duvara yapışık zıplarsan duvara kafa atmazsın)
        float iceriPayi = 0.15f; 
        
        Vector2 center = col.bounds.center;
        float topY = col.bounds.max.y;

        Vector2 topLeft = new Vector2(center.x - sideOffset + iceriPayi, topY);
        Vector2 topRight = new Vector2(center.x + sideOffset - iceriPayi, topY);

        // Işınları SADECE ceilingLayer'a atıyoruz
        RaycastHit2D hitL = Physics2D.Raycast(topLeft, Vector2.up, ceilingCheckDist, ceilingLayer);
        RaycastHit2D hitR = Physics2D.Raycast(topRight, Vector2.up, ceilingCheckDist, ceilingLayer);

        // Debug (Sarı = Boş, Kırmızı = Çarptı)
        Debug.DrawRay(topLeft, Vector2.up * ceilingCheckDist, hitL.collider != null ? Color.red : Color.yellow);
        Debug.DrawRay(topRight, Vector2.up * ceilingCheckDist, hitR.collider != null ? Color.red : Color.yellow);

        if (hitL.collider != null || hitR.collider != null)
        {
            // Yukarı hızı anında kes (Küt diye dur)
            velocity.y = 0;
            isJumping = false;

            // Kafa içine girdiyse aşağı it (Snap)
            RaycastHit2D hit = (hitL.collider != null) ? hitL : hitR;
            if (hitL.collider != null && hitR.collider != null)
            {
                hit = (hitL.distance < hitR.distance) ? hitL : hitR;
            }
            
            float distanceToCeiling = hit.distance;
            // Eğer mesafe çok azsa (içine girdiyse) aşağı it
            if (distanceToCeiling < 0.05f) 
            {
                rb.position = new Vector2(rb.position.x, rb.position.y - (0.05f - distanceToCeiling));
            }
        }
    }

    private void CheckWallCollision()
    {
        if (Mathf.Approximately(velocity.x, 0f)) return;

        float dir = Mathf.Sign(velocity.x);
        Vector2 origin = col.bounds.center;
        float checkDist = col.bounds.extents.x + 0.1f; 

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, checkDist, wallLayer);
        Debug.DrawRay(origin, Vector2.right * dir * checkDist, hit.collider != null ? Color.blue : Color.yellow);

        if (hit.collider != null)
        {
            velocity.x = 0;
        }
    }

    private void PerformGroundCheck()
    {
        // SlopeStabilizer ile uyumlu olması için fiziksel raycast her zaman dik atılır
        Vector2 center = col.bounds.center;
        float bottomY = col.bounds.min.y;

        // Ayakların biraz yukarısından başlatıyoruz (0.1f)
        Vector2 leftOrigin = new Vector2(center.x - sideOffset, bottomY + 0.1f);
        Vector2 rightOrigin = new Vector2(center.x + sideOffset, bottomY + 0.1f);

        RaycastHit2D hitL = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDist, groundLayer);
        RaycastHit2D hitR = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDist, groundLayer);

        Debug.DrawRay(leftOrigin, Vector2.down * groundCheckDist, hitL ? Color.green : Color.red);
        Debug.DrawRay(rightOrigin, Vector2.down * groundCheckDist, hitR ? Color.green : Color.red);

        RaycastHit2D hit = hitL ? hitL : hitR;

        if (hit.collider != null && velocity.y <= 0.1f)
        {
            isGrounded = true;
            velocity.y = 0;

            // Snap (Yere yapıştırma)
            float groundY = hit.point.y;
            float currentFeetY = col.bounds.min.y;
            float diff = groundY - currentFeetY;

            if (Mathf.Abs(diff) > 0.01f)
            {
                rb.position = new Vector2(rb.position.x, rb.position.y + diff);
            }

            if (controller != null) controller.ResetDash();
        }
        else
        {
            isGrounded = false;
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
