using UnityEngine;

public class MaskObject : MonoBehaviour
{
    public enum ObjectWorldType { Natural, MaskOnly }

    [Header("Dünya Ayarı")]
    [SerializeField] private ObjectWorldType worldType = ObjectWorldType.Natural;
    
    [Header("Görsel Ayarlar (Yok Olduğunda)")]
    [Range(0, 1)]
    [SerializeField] private float fadedAlpha = 0.3f; 
    [SerializeField] private Color fadedColor = Color.blue; 

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
        bool shouldBeSolid = (worldType == ObjectWorldType.Natural) ? !isMaskOn : isMaskOn;

        if (shouldBeSolid)
        {
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            if (col != null) col.enabled = true;
        }
        else
        {
            if (spriteRenderer != null)
            {
                Color tempColor = fadedColor;
                tempColor.a = fadedAlpha;
                spriteRenderer.color = tempColor;
            }
            if (col != null) col.enabled = false;
        }
    }

    public ObjectWorldType GetWorldType() => worldType;

    private void OnDestroy()
    {
        if (MaskManager.Instance != null)
            MaskManager.Instance.onMaskChanged -= UpdateObjectStatus;
    }
}