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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        RefreshObjectList();
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
    }

    private bool IsInsideAnyWall()
    {
        if (player == null) return false;

        Collider2D playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) return false;

        Bounds pBounds = playerCol.bounds;
        pBounds.Expand(3.6f); // Kenar kaçaklarını engellemek için genişletme

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

    public bool IsMaskActive() => isMaskOn;
}