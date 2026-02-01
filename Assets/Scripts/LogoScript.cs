using UnityEngine;

public class LogoScript : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 baslangicBoyutu;
    private Vector2 baslangicPozisyonu;

    [Header("Kalp Atışı Ayarları (BPM)")]
    [Tooltip("Dakikadaki vuruş sayısı. Örn: 60 = saniyede 1 vuruş.")]
    public float bpm = 60f; 
    
    [Tooltip("Ne kadar büyüyecek? (Örn: 0.1 yaparsan %10 büyür)")]
    public float atisGucu = 0.1f;

    [Header("Süzülme (Floating)")]
    public float suzulmeHizi = 1.0f;
    public float suzulmeMiktari = 15f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        baslangicBoyutu = rectTransform.localScale;
        baslangicPozisyonu = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // --- 1. KISIM: BPM EŞLİKLİ KALP ATIŞI ---
        
        // Matematiksel Formül: (Zaman * 2 * PI) * (BPM / 60)
        // Bu formül, sinüs dalgasının tam olarak BPM hızında dönmesini sağlar.
        float zamanFaktoru = Time.time * 2 * Mathf.PI * (bpm / 60f);

        // Mathf.Sin normalde -1 ile 1 arasında gider gelir (büyür/küçülür).
        // Kalp atışı hissi için 'küt-küt' etkisi yaratmak adına 
        // hafif bir modifikasyon yapabiliriz ama şimdilik pürüzsüz nefes alma (Sin) kullanıyoruz.
        float atisSinus = Mathf.Sin(zamanFaktoru) * atisGucu;

        // Orijinal boyuta ekliyoruz
        rectTransform.localScale = baslangicBoyutu + (Vector3.one * atisSinus);


        // --- 2. KISIM: SÜZÜLME (Bağımsız Hareket) ---
        // Süzülme BPM'den bağımsız olmalı ki logo "canlı" ama "robotik olmayan" dursun.
        float suzulmeSinus = Mathf.Sin(Time.time * suzulmeHizi) * suzulmeMiktari;
        
        rectTransform.anchoredPosition = baslangicPozisyonu + new Vector2(0f, suzulmeSinus);
    }
}
