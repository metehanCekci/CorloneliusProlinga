using UnityEngine;

public class MaskObject : MonoBehaviour
{
    public enum ObjectWorldType { Natural, MaskOnly }

    [Header("Dünya Ayarı")]
    [SerializeField] private ObjectWorldType worldType = ObjectWorldType.Natural;
    [SerializeField] private bool alwaysShow = false; // Her zaman görünür (solmaz)
    
    [Header("Görsel Ayarlar (Yok Olduğunda)")]
    [Range(0, 1)]
    [SerializeField] private float fadedAlpha = 0.3f; 
    [SerializeField] private Color fadedColor = Color.blue; 

    [Header("Ek Ayarlar")]
    [SerializeField] private bool alwaysSolid = false; // Her zaman collider açık kalsın mı?

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Color originalColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.onMaskChanged += UpdateObjectStatus;
            UpdateObjectStatus(MaskManager.Instance.IsMaskActive());
        }
        else
        {
            Debug.LogError(gameObject.name + ": MaskManager bulunamadı!");
        }
    }

    public void UpdateObjectStatus(bool isMaskOn)
    {
        bool shouldBeSolid = alwaysSolid || ((worldType == ObjectWorldType.Natural) ? !isMaskOn : isMaskOn);

        Debug.Log($"Updating MaskObject: {gameObject.name}, isMaskOn: {isMaskOn}, shouldBeSolid: {shouldBeSolid}, WorldType: {worldType}, AlwaysSolid: {alwaysSolid}");

        if (shouldBeSolid)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                Debug.Log($"{gameObject.name}: Setting color to original and enabling collider.");
            }
            if (col != null)
            {
                col.enabled = true;
                Debug.Log($"{gameObject.name}: Collider enabled.");
            }
        }
        else
        {
            // alwaysShow açıksa görsel solmaz, sadece collider kapanır
            if (spriteRenderer != null && !alwaysShow)
            {
                Color tempColor = fadedColor;
                tempColor.a = fadedAlpha;
                spriteRenderer.color = tempColor;
                Debug.Log($"{gameObject.name}: Setting color to faded.");
            }
            if (col != null)
            {
                col.enabled = false;
                Debug.Log($"{gameObject.name}: Collider disabled.");
            }
        }
    }

    public ObjectWorldType GetWorldType() => worldType;

    public bool IsSolid()
    {
        // Collider'ın açık olup olmadığını kontrol et
        return col != null && col.enabled;
    }

    private void OnDestroy()
    {
        if (MaskManager.Instance != null)
            MaskManager.Instance.onMaskChanged -= UpdateObjectStatus;
    }
}