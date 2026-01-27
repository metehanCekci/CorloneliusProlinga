using UnityEngine;

public class MaskObject : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private bool showWhenMaskIsOn; // İşaretliyse maske takınca görünür, değilse maske çıkınca görünür.
    
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // MaskManager'daki event'e abone ol
        MaskManager.Instance.onMaskChanged += UpdateObjectStatus;

        // Başlangıç durumunu ayarla
        UpdateObjectStatus(MaskManager.Instance.IsMaskActive());
    }

    private void UpdateObjectStatus(bool isMaskOn)
    {
        // Eğer objenin ayarı maske açıkken görünmekse ve maske açıksa: AKTİF
        // Diğer durumlarda objeyi kapat veya aç
        bool shouldBeActive = (isMaskOn == showWhenMaskIsOn);

        spriteRenderer.enabled = shouldBeActive; // Görseli kapat/aç
        if (col != null) col.enabled = shouldBeActive; // Çarpışmayı kapat/aç
    }

    private void OnDestroy()
    {
        // Bellek sızıntısını önlemek için event aboneliğinden çık
        if (MaskManager.Instance != null)
            MaskManager.Instance.onMaskChanged -= UpdateObjectStatus;
    }
}