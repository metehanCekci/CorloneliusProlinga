using UnityEngine;
using UnityEngine.SceneManagement; // 1. BU KÜTÜPHANEYÝ MUTLAKA EKLE

public class KeepAlive : MonoBehaviour
{
    public static KeepAlive Instance;

    void Awake()
    {
        // --- 1. KONTROL: Eðer oyun direkt Scene 0'da baþladýysa ve bu obje oradaysa ---
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Destroy(gameObject);
            return; // Kodu burada kes, aþaðýya inmesin
        }

        // --- SINGLETON MANTIÐI ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 2. Sahne deðiþtiðinde haber ver (Abone oluyoruz)
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Bu fonksiyon her sahne yüklendiðinde Unity tarafýndan otomatik çaðrýlýr
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Eðer yüklenen sahnenin numarasý (Build Index) 0 ise
        if (scene.buildIndex == 0)
        {
            // Aboneliði iptal et (Hata vermesin diye)
            SceneManager.sceneLoaded -= OnSceneLoaded;

            // Eðer Instance ben isem, statik referansý boþa çýkar
            if (Instance == this) Instance = null;

            // Kendini yok et
            Destroy(gameObject);
        }
    }

    // Obje yok olurken her ihtimale karþý aboneliði temizleyelim
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}