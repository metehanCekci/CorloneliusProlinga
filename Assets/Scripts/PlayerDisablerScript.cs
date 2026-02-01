using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneInputBlocker : MonoBehaviour
{
    public ControllerScript targetPlayer;

    private void Start()
    {
        // Player atanmadıysa otomatik bul
        if (targetPlayer == null) 
            targetPlayer = FindObjectOfType<ControllerScript>();

        DisableAllControls();
    }

    public void DisableAllControls()
    {
        if (targetPlayer == null) return;

        // 1. OYUNCUNUN KENDİ INPUTUNU KAPAT
        // (Yürüme, zıplama vb. durur)
        var pInput = targetPlayer.GetComponent<PlayerInput>();
        if (pInput != null) pInput.DeactivateInput();

        // 2. RAY INPUTUNU KAPAT
        // (Eğer raydaysa 'A' ve 'D' tuşlarını ray okumasın diye susturuyoruz)
        if (targetPlayer.activeRail != null)
        {
            targetPlayer.activeRail.SetRailInput(false);
        }
    }

    // Cutscene bitince tekrar açmak istersen:
    public void EnableAllControls()
    {
        if (targetPlayer == null) return;

        var pInput = targetPlayer.GetComponent<PlayerInput>();
        if (pInput != null) pInput.ActivateInput();

        if (targetPlayer.activeRail != null)
        {
            targetPlayer.activeRail.SetRailInput(true);
        }
    }
}
