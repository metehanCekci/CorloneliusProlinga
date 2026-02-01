using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool disableOnCollect = true; // Alınınca kapansın mı?

    private SpriteRenderer spriteRenderer;
    private Collider2D checkpointCol;
    private Animator animator;
    private bool isCollected = false;
    private readonly string pickupTrigger = "Pickup";

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        checkpointCol = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Katman kontrolü ve daha önce alınıp alınmadığı
        if (!isCollected && ((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Oyuncu scriptine ulaşıp sesi çaldıralım
            if (collision.TryGetComponent<ControllerScript>(out ControllerScript player))
            {
                if (player.soundManager != null)
                {
                    player.soundManager.PlayPickup(); // Checkpoint için de pickup sesi çalar
                }
            }

            CollectCheckpoint();
        }
    }

    private void CollectCheckpoint()
    {
        isCollected = true;

        // Burada eğer bir GameManager kullanıyorsan spawn noktasını güncelleyebilirsin
        // GameManager.Instance.UpdateCheckpoint(transform.position);

        Debug.Log("Checkpoint Alındı: " + gameObject.name);

        if (animator != null)
        {
            animator.SetTrigger(pickupTrigger);
        }
        if (disableOnCollect)
        {
            // Objenin görselini ve çarpışmasını kapatır ama script çalışmaya devam eder
            if (checkpointCol != null) checkpointCol.enabled = false;
            // Eğer objeyi tamamen hiyerarşiden silmek istersen:
            // Destroy(gameObject); 
        }
    }

    // Eğer oyuncu öldüğünde checkpoint'lerin geri gelmesini istersen bu metodu çağırabilirsin
    public void ResetCheckpoint()
    {
        isCollected = false;
        if (checkpointCol != null) checkpointCol.enabled = true;
    }
}