using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float gravityStrength = 5f; // Düşüş hızı
    [SerializeField] private float groundRayDistance = 0.1f; // Yer kontrolü mesafesi
    [SerializeField] private float jumpForce = 10f; // Zıplama kuvveti
    [SerializeField] private float maxJumpHeight = 5f; // Maksimum zıplama yüksekliği
    
    private Vector3 velocity = Vector3.zero;
    private bool isGrounded = false;
    private bool isJumping = false;
    private float jumpStartHeight = 0f;
    private Collider2D col;

    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Yeri kontrol et
        CheckGround();

        // Jump mekanikler
        if (isJumping)
        {
            // Space basılı → yukarı git
            velocity.y = jumpForce;
            
            // Max yüksekliğe ulaştıysa jump'ı bitir
            float currentHeight = transform.position.y - jumpStartHeight;
            if (currentHeight >= maxJumpHeight)
            {
                isJumping = false;
            }
        }
        else
        {
            // Space bırakıldı → gravity uygulanıyor
            if (isGrounded && velocity.y <= 0)
            {
                velocity.y = 0;
            }
            else
            {
                velocity.y -= gravityStrength * Time.deltaTime;
            }
        }

        // Pozisyon güncelleniyor
        transform.position += velocity * Time.deltaTime;
    }

    // Yeri kontrol et (Raycast)
    private void CheckGround()
    {
        int groundLayer = LayerMask.GetMask("Ground");
        Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundRayDistance, groundLayer);
        
        isGrounded = hit.collider != null;
    }

    // Jump başlat
    public void StartJump()
    {
        if (isGrounded)
        {
            isJumping = true;
            jumpStartHeight = transform.position.y;
        }
    }

    // Jump sonlandır
    public void EndJump()
    {
        isJumping = false;
    }

    // Hız değerini alma
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // Hız değerini ayarlama
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }

    // Yer kontrolü sorgusu
    public bool IsGrounded()
    {
        return isGrounded;
    }
}
