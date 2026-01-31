using UnityEngine;

public class DashCrystal : MonoBehaviour
{
    [Header("Gorsel Ayarlar")]
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

    [Header("Manuel Kontrol Ayarlari")]
    [SerializeField] private LayerMask playerLayer; 

    private SpriteRenderer spriteRenderer;
    private Collider2D crystalCol;
    private Animator animator; // Animator referansı eklendi
    private bool isAvailable = true;
    private ControllerScript playerScript;

    // Animator parametre isimleri (Animator'da oluşturduğun isimlerle aynı olmalı)
    private readonly string pickupTrigger = "Pickup"; 
    private readonly string respawnTrigger = "Respawn";

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        crystalCol = GetComponent<Collider2D>();
        animator = GetComponent<Animator>(); // Animator'ı yakalıyoruz
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
            if (!player.HasDash || !player.HasSecondJump)
            {
                playerScript = player;
                CollectCrystal();
            }
        }
    }

    private void CollectCrystal()
    {
        isAvailable = false;
        
        if (spriteRenderer != null) spriteRenderer.color = inactiveColor;
        
        // Toplama animasyonunu tetikle
        if (animator != null)
        {
            animator.SetTrigger(pickupTrigger);
        }

        playerScript.HasDash = true;
        playerScript.HasSecondJump = true;
        
        Debug.Log("Kristal manuel olarak toplandi ve oyuncuyu yeniledi!");
    }

    private void ResetCrystal()
    {
        isAvailable = true;
        
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        // Yeniden canlanma animasyonunu tetikle
        if (animator != null)
        {
            animator.SetTrigger(respawnTrigger);
        }
    }
}