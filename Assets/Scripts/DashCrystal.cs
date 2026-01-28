using UnityEngine;

public class DashCrystal : MonoBehaviour
{
    [Header("Gorsel Ayarlar")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

    [Header("Manuel Kontrol Ayarlari")]
    [SerializeField] private LayerMask playerLayer; 

    private SpriteRenderer spriteRenderer;
    private Collider2D crystalCol;
    private bool isAvailable = true;
    private ControllerScript playerScript;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        crystalCol = GetComponent<Collider2D>();
        spriteRenderer.color = activeColor;
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

        playerScript.HasDash = true;
        playerScript.HasSecondJump = true;
        
        Debug.Log("Kristal manuel olarak toplandi ve oyuncuyu yeniledi!");
    }

    private void ResetCrystal()
    {
        isAvailable = true;
        if (spriteRenderer != null) spriteRenderer.color = activeColor;
    }
}