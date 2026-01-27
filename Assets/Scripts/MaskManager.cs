using UnityEngine;
using UnityEngine.InputSystem;

public partial class MaskManager : MonoBehaviour
{
    public static MaskManager Instance; // Diğer objelerin kolayca erişmesi için

    [SerializeField] private bool isMaskOn = false;
    private InputActions inputActions;
    private InputAction maskAction;

    // Görsel efektler veya sesler için event (Opsiyonel ama GameJam'de puan kazandırır)
    public delegate void OnMaskChanged(bool isOn);
    public event OnMaskChanged onMaskChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        
        // Input Actions'ta "Mask" adında bir Action oluşturduğunu varsayıyorum (Örn: 'E' tuşu veya 'R1')
        maskAction = inputActions.Player.Mask; 
        maskAction.performed += ToggleMask;
    }

    private void OnDisable()
    {
        maskAction.performed -= ToggleMask;
        inputActions.Disable();
    }

    private void ToggleMask(InputAction.CallbackContext context)
    {
        isMaskOn = !isMaskOn;
        Debug.Log("Maske Durumu: " + (isMaskOn ? "Takılı" : "Çıkarıldı"));

        // Etraftaki tüm 'Maske Objeleri'ne haber ver
        onMaskChanged?.Invoke(isMaskOn);
    }

    public bool IsMaskActive() => isMaskOn;
}