using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI Ayarları")]
    public GameObject pauseMenuPanel;
    public InputActionReference pauseAction;

    [Header("Oyuncu Kontrolü")]
    // Buraya karakterin üzerindeki hareket scriptini (ControllerScript) sürükle
    public MonoBehaviour playerScript; 

    private bool isPaused = false;

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += _ => TogglePause();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Disable();
            pauseAction.action.performed -= _ => TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Zamanı durdur
        pauseMenuPanel.SetActive(true);

        // İSTEDİĞİN KISIM: Sadece scripti kapatıyoruz.
        // Böylece Update durur, karakter hareket komutu almaz.
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Zamanı başlat
        pauseMenuPanel.SetActive(false);

        // Scripti tekrar açıyoruz
        if (playerScript != null)
        {
            playerScript.enabled = true;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // MAIN MENU TUŞU İÇİN
    public void LoadMenu()
    {
        // 1. Önce zamanı normale döndür
        Time.timeScale = 1f;

        // 2. Sahne geçiş scriptini bul (İsmi FadeScript idi)
        FadeScript transition = FindObjectOfType<FadeScript>();
        
        if (transition != null)
        {
            // 0 numaralı index genellikle Main Menu'dür.
            transition.BaslatFadeOut(0); 
        }
        else
        {
            Debug.LogError("Hata: Sahnede 'FadeScript' scripti bulunamadı!");
            // Acil durum çıkışı
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
