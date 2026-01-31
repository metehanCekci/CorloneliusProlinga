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
    [SerializeField] private float ceilingCheckDist = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    // Tavanı ayrı kontrol etmek istersen diye (yoksa Start'ta groundLayer ile eşitliyoruz)
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

        // Tavan layer seçilmediyse ground kabul etsin
        if (ceilingLayer == 0) ceilingLayer = groundLayer;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        // Grind veya Duvardaysak yer çekimi/zemin kontrolü yapma
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
            PerformGroundCheck();
            // Karakteri dik tut
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.fixedDeltaTime * rotationSpeed);
        }

        HandleVerticalMovement();

        // --- TAVAN KONTROLÜ ---
        CheckCeilingCollision();

        // --- DİĞER KONTROLLER ---
        CheckWallCollision();
        CheckRailCollision();

        // Hareketi uygula
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void CheckCeilingCollision()
    {
        // Sadece yukarı çıkarken kafa atarız
        if (velocity.y <= 0) return;

        float iceriPayi = 0.15f;
        Vector2 center = col.bounds.center;
        float topY = col.bounds.max.y;

        Vector2 topLeft = new Vector2(center.x - sideOffset + iceriPayi, topY);
        Vector2 topRight = new Vector2(center.x + sideOffset - iceriPayi, topY);

        RaycastHit2D hitL = Physics2D.Raycast(topLeft, Vector2.up, ceilingCheckDist, ceilingLayer);
        RaycastHit2D hitR = Physics2D.Raycast(topRight, Vector2.up, ceilingCheckDist, ceilingLayer);

        if (hitL.collider != null || hitR.collider != null)
        {
            // Kafayı vurduk, hızı kes
            velocity.y = 0;
            isJumping = false;

            // İçine girdiysek aşağı it (Snap)
            RaycastHit2D hit = (hitL.collider != null) ? hitL : hitR;
            if (hitL.collider != null && hitR.collider != null)
            {
                hit = (hitL.distance < hitR.distance) ? hitL : hitR;
            }

            float distanceToCeiling = hit.distance;
            if (distanceToCeiling < 0.05f)
            {
                rb.position = new Vector2(rb.position.x, rb.position.y - (0.05f - distanceToCeiling));
            }
        }
    }

    private void CheckWallCollision()
    {
        // Hareket yoksa bakma
        if (Mathf.Approximately(velocity.x, 0f)) return;

        float dir = Mathf.Sign(velocity.x);
        Vector2 origin = col.bounds.center;
        float checkDist = col.bounds.extents.x + 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, checkDist, wallLayer);

        if (hit.collider != null)
        {
            velocity.x = 0;
        }
    }

    private void PerformGroundCheck()
    {
        Vector2 center = col.bounds.center;
        float bottomY = col.bounds.min.y;
        Vector2 leftOrigin = new Vector2(center.x - sideOffset, bottomY + 0.1f);
        Vector2 rightOrigin = new Vector2(center.x + sideOffset, bottomY + 0.1f);

        RaycastHit2D hitL = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDist, groundLayer);
        RaycastHit2D hitR = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDist, groundLayer);

        RaycastHit2D hit = hitL ? hitL : hitR;

        if (hit.collider != null && velocity.y <= 0.1f)
        {
            // === ÖNEMLİ: ONLAND ÇAĞRISI ===
            // Havadaydık ve şimdi yere değdik. Hızı sıfırlamadan önce controller'a bildir.
            if (!isGrounded && controller != null)
            {
                controller.OnLand(velocity.y);
            }
            // ==============================

            isGrounded = true;
            velocity.y = 0; // Hız şimdi sıfırlanıyor

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
            // Maksimum yüksekliğe ulaştı mı?
            if (rb.position.y - jumpStartHeight >= maxJumpHeight) isJumping = false;
        }
        else if (!isGrounded)
        {
            // Yer çekimi uygula
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
        groundCooldown = 0.2f; // Zıpladığımız an yer çekimi kontrolünü kısa süre kapat
        isGrounded = false;
    }

    public void EndJump() => isJumping = false;

    public void SetVelocity(Vector2 newVel)
    {
        velocity = newVel;
        // Eğer yukarı doğru bir hız verildiyse (dash, zıplama vb) ground check'i geçici kapat
        if (newVel.y > 0) groundCooldown = 0.2f;
    }

    public Vector2 GetVelocity() => velocity;
    public bool IsGrounded() => isGrounded;
}