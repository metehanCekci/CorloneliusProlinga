using UnityEngine;

public class DashCrystal : MonoBehaviour
{
    [Header("Gorsel Ayarlar")]
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

    [Header("Manuel Kontrol Ayarlari")]
    [SerializeField] private LayerMask playerLayer; 

    private SpriteRenderer spriteRenderer;
    private Collider2D crystalCol;
    private Animator animator; 
    private bool isAvailable = true;
    private ControllerScript playerScript;

    private readonly string pickupTrigger = "Pickup"; 
    private readonly string respawnTrigger = "Respawn";

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        crystalCol = GetComponent<Collider2D>();
        animator = GetComponent<Animator>(); 
    }

    private void Update()
    {
        if (isAvailable)
        {
            CheckPlayerCollision();
        }
        else if (playerScript != null && playerScript.IsGrounded())
        {
            ResetCrystal();
        }
    }

    private void CheckPlayerCollision()
    {
        Collider2D hit = Physics2D.OverlapBox(crystalCol.bounds.center, crystalCol.bounds.size, 0f, playerLayer);

        if (hit != null && hit.TryGetComponent<ControllerScript>(out ControllerScript player))
        {
            // DEĞİŞİKLİK BURADA: 
            // Sadece oyuncunun Dash hakkı bitmişse (HasDash == false) kristal toplanır.
            // Eğer oyuncunun dash'i zaten varsa, kristal onu görmezden gelir.
            if (!player.HasDash)
            {
                playerScript = player;
                CollectCrystal();
            }
        }
    }

   private void CollectCrystal()
    {
        isAvailable = false;
        
        if (spriteRenderer != null) spriteRenderer.enabled = false; 
        
        if (animator != null)
        {
            animator.SetTrigger(pickupTrigger);
        }

        // Kristal alındığında her iki hakkı da doldurur
        playerScript.HasDash = true;
        playerScript.HasSecondJump = true;
    }

    private void ResetCrystal()
    {
        isAvailable = true;
        
        if (spriteRenderer != null) 
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
        }

        if (animator != null)
        {
            animator.SetTrigger(respawnTrigger);
        }
    }
}