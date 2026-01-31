using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance;

    [Header("Mask Ayarları")]
    [SerializeField] private bool isMaskOn = false;

    [Header("Referanslar")]
    [SerializeField] private GameObject player;
    private SpriteRenderer playerSprite; 

    private InputActions inputActions;
    private InputAction maskAction;
    private List<MaskObject> allMaskObjects = new List<MaskObject>();

    public delegate void OnMaskChanged(bool isOn);
    public event OnMaskChanged onMaskChanged;

    [Header("Efektler")]
    public EffectsManager effectsManager;
    public LightManager lightManager;

    // Hedef Rengi Cache'lemek için
    private Color maskOnColor;
    private Color maskOffColor = Color.white; 

    // --- OUTLINE İÇİN DEĞİŞKENLER (DEĞİŞTİ) ---
    // MaterialPropertyBlock sildik, yerine Hayalet Obje referansları geldi
    private GameObject outlineObject; 
    private SpriteRenderer outlineSprite;
    
    [Header("Outline Ayarları")]
    public Color outlineColor = Color.green; 
    public float outlineThickness = 1f; // Bu artık büyüklük çarpanı olarak çalışacak (Örn: 1.15f)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        ColorUtility.TryParseHtmlString("#40E0D8", out maskOnColor);
        outlineColor = maskOnColor; 

        RefreshObjectList();
    }

    void Start() 
    {
        if (player != null)
        {
            playerSprite = player.GetComponentInChildren<SpriteRenderer>();
            // PropBlock sildik, yerine Outline objesini yaratıyoruz
            CreateOutlineObject(); 
        }
    }

    // --- YENİ EKLENEN: GHOST OBJE YARATMA ---
    private void CreateOutlineObject()
    {
        // 1. Yeni boş bir obje oluştur
        outlineObject = new GameObject("PlayerOutline_Ghost");
        // 2. Player ile aynı hiyerarşiye koy (Düzenli dursun)
        outlineObject.transform.SetParent(player.transform.parent); 
        
        // 3. Sprite Renderer ekle
        outlineSprite = outlineObject.AddComponent<SpriteRenderer>();
        
        // 4. Ayarları yap
        outlineSprite.color = outlineColor; // Belirlediğin renk (#40E0D8)
        outlineSprite.sortingLayerName = playerSprite.sortingLayerName;
        outlineSprite.sortingOrder = playerSprite.sortingOrder - 1; // Karakterin ARKASINA koy
        
        // Default materyal kullan (Shader derdi yok)
        outlineSprite.material = new Material(Shader.Find("Sprites/Default")); 
        
        // Başlangıçta gizle
        outlineObject.SetActive(false);
    }

    // --- YENİ EKLENEN: SÜREKLİ TAKİP ---
    void Update()
    {
        // Eğer outline açıksa (aktifse), sürekli ana karakteri kopyalasın
        if (outlineObject != null && outlineObject.activeSelf && playerSprite != null)
        {
            // Pozisyon ve Dönüşü eşitle
            outlineObject.transform.position = player.transform.position;
            outlineObject.transform.rotation = player.transform.rotation;
            
            // Büyüklüğü ayarla (Karakterden biraz daha büyük olsun ki arkadan taşsın)
            // 0.15f ekleyerek %15 daha büyük yapıyoruz (outlineThickness burada işe yarıyor)
            float scaleMultiplier = 1f + (outlineThickness * 0.15f); 
            outlineObject.transform.localScale = player.transform.localScale * scaleMultiplier;
            
            // Sprite ve Flip durumunu anlık kopyala (Animasyon aynen görünür)
            outlineSprite.sprite = playerSprite.sprite;
            outlineSprite.flipX = playerSprite.flipX;
            outlineSprite.flipY = playerSprite.flipY;
        }
    }

    public void RefreshObjectList()
    {
        allMaskObjects.Clear();
        allMaskObjects.AddRange(Object.FindObjectsByType<MaskObject>(FindObjectsSortMode.None));
        Debug.Log("Sistem: " + allMaskObjects.Count + " adet obje takip listesine alındı.");
    }

    private void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        maskAction = inputActions.Player.Mask;
        maskAction.performed += ToggleMask;
    }

    private void OnDisable()
    {
        maskAction.performed -= ToggleMask;
        inputActions.Disable();
        inputActions.Dispose();
    }

    private void ToggleMask(InputAction.CallbackContext context)
    {
        if (isMaskOn && IsInsideAnyWall())
        {
            Debug.LogError("!!! MASKMGR: DUVARIN İÇİNDESİN, KAPATMA ENGELLENDİ !!!");
            return;
        }

        isMaskOn = !isMaskOn;
        Debug.Log("MASKMGR: Maske durumu değiştirildi -> " + (isMaskOn ? "Açık" : "Kapalı"));
        onMaskChanged?.Invoke(isMaskOn);

        // --- OUTLINE GÜNCELLEME ---
        UpdateOutline(isMaskOn); 

        if (lightManager != null)
        {
            if (isMaskOn) lightManager.ChangeAtmosphere(1f, maskOnColor, 0.5f);
            else lightManager.ChangeAtmosphere(1f, maskOffColor, 0.5f);
        }

        if (effectsManager != null) effectsManager.VurusEfekti(1f);
    }

    // --- GÜNCELLENEN FONKSİYON ---
    // Artık Shader parametresi değil, objenin kendisini açıp kapatıyor
    private void UpdateOutline(bool active)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(active);
        }
    }

    private bool IsInsideAnyWall()
    {
        if (player == null) return false;

        Collider2D playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) return false;

        Bounds pBounds = playerCol.bounds;
        pBounds.Expand(0.6f); 

        foreach (var obj in allMaskObjects)
        {
            if (obj.GetWorldType() == MaskObject.ObjectWorldType.Natural)
            {
                Collider2D wallCol = obj.GetComponent<Collider2D>();
                if (wallCol != null)
                {
                    if (pBounds.Intersects(wallCol.bounds))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void ResetMaskToDefault()
    {
        if (!isMaskOn) return;

        isMaskOn = false;
        Debug.Log("MASKMGR: Ölüm sonrası maske sıfırlandı -> Kapalı");

        onMaskChanged?.Invoke(isMaskOn);
        
        if (lightManager != null) lightManager.ChangeAtmosphere(1f, maskOffColor, 0.1f); // Hızlı reset

        // Outline'ı da sıfırla
        UpdateOutline(false);
    }

    public bool IsMaskActive() => isMaskOn;
}
