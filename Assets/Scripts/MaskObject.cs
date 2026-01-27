using UnityEngine;

public class MaskObject : MonoBehaviour
{
    // Radio button mantığı için enum kullanıyoruz
    public enum ObjectWorldType
    {
        Natural, // Maske yokken var olanlar
        MaskOnly // Maske takılıyken var olanlar
    }

    [Header("Dünya Ayarı")]
    [SerializeField] private ObjectWorldType worldType = ObjectWorldType.Natural; // Default olarak Natural

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // MaskManager'a abone ol
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.onMaskChanged += UpdateObjectStatus;
            // Başlangıç durumunu ayarla
            UpdateObjectStatus(MaskManager.Instance.IsMaskActive());
        }
    }

    private void UpdateObjectStatus(bool isMaskOn)
    {
        bool shouldBeActive = false;

        // Seçime göre mantığı kur
        if (worldType == ObjectWorldType.Natural)
        {
            // Eğer natural seçildiyse, maske kapalıyken aktif olmalı
            shouldBeActive = !isMaskOn;
        }
        else if (worldType == ObjectWorldType.MaskOnly)
        {
            // Eğer maske dünyası seçildiyse, maske açıkken aktif olmalı
            shouldBeActive = isMaskOn;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = shouldBeActive;
        if (col != null) col.enabled = shouldBeActive;
    }

    private void OnDestroy()
    {
        if (MaskManager.Instance != null)
            MaskManager.Instance.onMaskChanged -= UpdateObjectStatus;
    }
}