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
    private float groundCheckCooldown = 0f;
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
        if (controller != null && (controller.isGrinding || controller.IsOnWall()))
        {
            isGrounded = false;
            return;
        }

        // Ground check'i her zaman en başta yap
        if (groundCheckCooldown > 0)
        {
            groundCheckCooldown -= Time.deltaTime;
            isGrounded = false;
        }
        else
        {
            CheckGround();
        }

        if (isJumping)
        {
            velocity.y = jumpForce;
            if (transform.position.y - jumpStartHeight >= maxJumpHeight) isJumping = false;
        }
        else
        {
            // Eğer yerdeysek hızı biriktirmeyi bırak, dikey hızı kilitle
            if (isGrounded && velocity.y <= 0)
            {
                velocity.y = 0;
            }
            else
            {
                velocity.y -= gravityStrength * Time.deltaTime;
            }
        }

        CheckRailCollision();
        CheckWallCollision();

        // Hareketi en son uygula
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
                float entrySpeed = Mathf.Abs(velocity.x);
                float entryDirection = velocity.x;
                rail.StartGrindFromManual(controller, entrySpeed, entryDirection);
            }
        }
    }

private void CheckGround()
{
    if (col == null) return;

    float checkDist = Mathf.Max(groundRayDistance, Mathf.Abs(velocity.y * Time.deltaTime) + 0.05f);
    Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y + 0.1f);
    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, checkDist, groundLayer);

    if (hit.collider != null && velocity.y <= 0.01f)
    {
        isGrounded = true;

        float groundY = hit.point.y;
        float feetOffset = col.bounds.min.y - transform.position.y;
        float targetY = groundY - feetOffset;

        // --- DEĞİŞEN KISIM BURASI ---
        // Eğer karakter yerin çok altındaysa (0.05'ten fazla) sadece o zaman müdahale et
        // Ve yukarı snaplerken çok küçük bir pay bırak (0.01f)
        if (transform.position.y < targetY - 0.01f)
        {
            // Çat diye ışınlamak yerine sadece olması gereken yere çok yakın bir noktaya çek
            transform.position = new Vector3(transform.position.x, targetY + 0.01f, transform.position.z);
        }
        
        velocity.y = 0; // Hızı kes ki daha fazla batmasın
        // ----------------------------

        if (controller != null)
        {
            controller.ResetDash();
        }
    }
    else
    {
        isGrounded = false;
    }
}

    public void StartJump() { isJumping = true; jumpStartHeight = transform.position.y; }
    public void EndJump() { isJumping = false; }
    public void SetVelocity(Vector3 newVel)
    {
        velocity = newVel;
        if (newVel.y > 0) groundCheckCooldown = 0.15f;
    }
    public Vector3 GetVelocity() => velocity;
    public bool IsGrounded() => isGrounded;

    public void ApplyWallMovement()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
