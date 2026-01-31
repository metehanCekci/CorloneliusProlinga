using UnityEngine;
using UnityEngine.Rendering;                 // Volume erişimi için
using UnityEngine.Rendering.Universal;         // URP efektleri için

public class EffectsManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public Volume globalVolume; // Inspector'dan Global Volume objeni buraya sürükle
    public float toparlanmaHizi = 5f; // Efektin ne kadar çabuk silineceği (Büyük sayı = hızlı silinir)

    // Private değişkenler (Cache)
    private ChromaticAberration chroma;
    private LensDistortion lensDist; // İstersen bunu da kullanabilirsin

    void Start()
    {
        if (globalVolume.profile.TryGet(out chroma))
        {
            // TİK ATMA KOMUTU: Intensity'nin kontrolünü script'e ver
            chroma.intensity.overrideState = true; 
            chroma.active = true; // Efektin kendisini de aktif et
            chroma.intensity.value = 0f;
        }
        
        if (globalVolume.profile.TryGet(out lensDist))
        {
            lensDist.intensity.overrideState = true;
            lensDist.active = true;
            lensDist.intensity.value = 0f;
        }
    }

    void Update()
    {
        // --- ZAMANLA AZALMA (RECOVERY) ---
        // Eğer efektin şiddeti 0'dan büyükse, zamanla yavaşça 0'a çekiyoruz.
        
        if (chroma != null && chroma.intensity.value > 0.01f)
        {
            chroma.intensity.value = Mathf.Lerp(chroma.intensity.value, 0f, Time.deltaTime * toparlanmaHizi);
        }

        // Lens Distortion varsa onu da düzelt
        if (lensDist != null && Mathf.Abs(lensDist.intensity.value) > 0.01f)
        {
            lensDist.intensity.value = Mathf.Lerp(lensDist.intensity.value, 0f, Time.deltaTime * toparlanmaHizi);
        }
    }

    // Bu fonksiyonu diğer scriptlerden çağıracağız
    public void VurusEfekti(float siddet)
    {
        Debug.Log("Vuruş Efekti Tetiklendi! Şiddet: " + siddet); 

        if (chroma != null) 
        {
            chroma.intensity.value = siddet; 
        }

        if (lensDist != null)
        {
            // Balık gözü için eksi değer genelde daha "hızlı" hissettirir (-0.5 gibi)
            lensDist.intensity.value = -siddet * 0.5f; 
        }
    }
}
