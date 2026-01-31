using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScript : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [Header("Ayarlar")]
    public float fadeSuresi = 1.0f;
    // Bu süre boyunca ekran simsiyah kalır, objeler ve kamera yerleşir.
    public float acilisGecikmesi = 0.5f;
    public bool mainMenu = true;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // Başlangıçta ekran simsiyah ve açık olsun
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1;

        // Hemen aydınlatma yapma, biraz bekle (Kamera yerine otursun)
        StartCoroutine(SahneAcilisGecikmesi());
    }

    // --- YENİ EKLENEN GECİKMELİ BAŞLANGIÇ ---
    IEnumerator SahneAcilisGecikmesi()
    {
        // Belirlenen süre kadar siyah ekranda bekle
        yield return new WaitForSeconds(acilisGecikmesi);

        // Süre bitince aydınlatmayı başlat
        StartCoroutine(FadeIslemi(1, 0));
    }

    // --- ÖZEL FONKSİYONLAR (DEATH SCRIPT İÇİN) ---

    // KARARTMA (Aynı kaldı)
    public IEnumerator FadeOutEkraniKarart(float sure)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true; // Tıklamayı engelle
        canvasGroup.alpha = 0;

        float counter = 0f;
        while (counter < sure)
        {
            counter += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / sure);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    // AYDINLATMA (ÖNEMLİ: Death Script'ten çağırırken bunu GECİKMELİ kullanmak isteyebilirsin)
    public IEnumerator FadeInEkraniAc(float sure, float beklemeSuresi = 0f)
    {
        // Eğer bir bekleme süresi varsa (kamera yetişsin diye), siyah ekranda bekle
        if (beklemeSuresi > 0)
        {
            canvasGroup.alpha = 1;
            yield return new WaitForSeconds(beklemeSuresi);
        }

        float counter = 0f;
        while (counter < sure)
        {
            counter += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, counter / sure);
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    // --- NORMAL SAHNE GEÇİŞLERİ (Aynı kaldı) ---

    public void SiradakiSahne()
    {
        Debug.Log("Toplam Sahne Sayısı: " + SceneManager.sceneCountInBuildSettings);
        int mevcutIndex = SceneManager.GetActiveScene().buildIndex;
        if (mevcutIndex + 1 < SceneManager.sceneCountInBuildSettings)
            BaslatFadeOut(mevcutIndex + 1);
        else
            Debug.Log("Başka sahne yok.");
    }

    public void BaslatFadeOut(int hedefIndex)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(FadeIslemi(0, 1, true, hedefIndex));
    }

    IEnumerator FadeIslemi(float start, float end, bool sahneYukle = false, int hedefIndex = -1)
    {
        float counter = 0f;
        while (counter < fadeSuresi)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / fadeSuresi);
            yield return null;
        }
        canvasGroup.alpha = end;

        if (!sahneYukle)
        {
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            SceneManager.LoadScene(hedefIndex);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}