using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private bool disableOnCollect = true; // Alınınca kapansın mı?

    private SpriteRenderer spriteRenderer;
    private Collider2D checkpointCol;
    private bool isCollected = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        checkpointCol = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Katman kontrolü ve daha önce alınıp alınmadığı
        if (!isCollected && ((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            CollectCheckpoint();
        }
    }

    private void CollectCheckpoint()
    {
        isCollected = true;
        
        // Burada eğer bir GameManager kullanıyorsan spawn noktasını güncelleyebilirsin
        // GameManager.Instance.UpdateCheckpoint(transform.position);

        Debug.Log("Checkpoint Alındı: " + gameObject.name);

        if (disableOnCollect)
        {
            // Objenin görselini ve çarpışmasını kapatır ama script çalışmaya devam eder
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (checkpointCol != null) checkpointCol.enabled = false;
            
            // Eğer objeyi tamamen hiyerarşiden silmek istersen:
            // Destroy(gameObject); 
        }
    }

    // Eğer oyuncu öldüğünde checkpoint'lerin geri gelmesini istersen bu metodu çağırabilirsin
    public void ResetCheckpoint()
    {
        isCollected = false;
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (checkpointCol != null) checkpointCol.enabled = true;
    }
}