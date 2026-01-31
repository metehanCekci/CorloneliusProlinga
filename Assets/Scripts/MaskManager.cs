using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance;

    [Header("Mask Ayarları")]
    [SerializeField] private bool isMaskOn = false;

    [Header("Sıkışma Engelleme Ayarları")]
    [SerializeField] private GameObject player;

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
    private Color maskOffColor = Color.white; // Normal hali beyaz (renksiz)
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        ColorUtility.TryParseHtmlString("#40E0D8", out maskOnColor);

        RefreshObjectList();
    }

    public void RefreshObjectList()
    {
        allMaskObjects.Clear();
        allMaskObjects.AddRange(Object.FindObjectsByType<MaskObject>(FindObjectsSortMode.None));
        Debug.Log("Sistem: " + allMaskObjects.Count + " adet obje takip listesine alındı.");

        foreach (var obj in allMaskObjects)
        {
            Debug.Log($"Tracked Object: {obj.gameObject.name}, WorldType: {obj.GetWorldType()}");
        }
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
        // --- IŞIK VE RENK DEĞİŞİMİ ---
        if (lightManager != null)
        {
            if (isMaskOn)
            {
                // MASKE AÇIK: Intensity 2.5, Renk #00FF08 (Yeşil), Süre 0.5sn (Hızlı geçiş)
                lightManager.ChangeAtmosphere(2.5f, maskOnColor, 0.5f);
            }
            else
            {
                // MASKE KAPALI: Intensity 1.0, Renk Beyaz, Süre 0.5sn
                lightManager.ChangeAtmosphere(1f, maskOffColor, 0.5f);
            }
        }

        // --- EKRAN BOZULMA EFEKTİ ---
        if (effectsManager != null)
        {
            effectsManager.VurusEfekti(1f); // 1f = Tam güç efekt (Maksimum bozulma)
        }

    }

    private bool IsInsideAnyWall()
    {
        if (player == null) return false;

        Collider2D playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) return false;

        Bounds pBounds = playerCol.bounds;
        pBounds.Expand(0.6f); // Kenar kaçaklarını engellemek için genişletme

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
        // Eğer maske zaten kapalıysa (false) işlem yapma, gereksiz olay tetikleme
        if (!isMaskOn) return;

        isMaskOn = false;
        Debug.Log("MASKMGR: Ölüm sonrası maske sıfırlandı -> Kapalı");

        // Tüm objelere maskenin kapandığını bildir
        onMaskChanged?.Invoke(isMaskOn);
    }

    public bool IsMaskActive() => isMaskOn;
}
