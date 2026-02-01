using UnityEngine;
using UnityEngine.SceneManagement;

public class KeepAlive : MonoBehaviour
{
    public static KeepAlive Instance;

    void Awake()
    {
        // Þu anki sahne ve Son sahne indexini alalým
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int lastSceneIndex = SceneManager.sceneCountInBuildSettings - 1; // Örn: 5 sahne varsa son index 4'tür.

        // --- 1. KONTROL: Baþlangýçta Scene 0'da VEYA Son Sahnede isek ---
        // Oyun direkt son sahnede baþlarsa da (test için) bu obje oluþmasýn.
        if (currentSceneIndex == 0 || currentSceneIndex == lastSceneIndex)
        {
            Destroy(gameObject);
            return;
        }

        // --- SINGLETON MANTIÐI ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Sahne deðiþimini dinlemeye baþla
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Son sahnenin indexini hesapla
        int lastSceneIndex = SceneManager.sceneCountInBuildSettings - 1;

        // --- YENÝ EKLENEN KISIM ---
        // Eðer yüklenen sahne 0 (Ana Menü) VEYA Son Sahne (Credits/Final) ise:
        if (scene.buildIndex == 0 || scene.buildIndex == lastSceneIndex)
        {
            // Aboneliði iptal et
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // Instance'ý boþa çýkar
            if (Instance == this) Instance = null;

            // Kendini yok et
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}