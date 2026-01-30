using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
    [Header("Ayarlar")]
    public AudioMixer audioMixer; // Yarattığın Mixer'i buraya sürükle
    public Slider masterSlider;   // Master Knob/Slider'ı buraya
    public Slider musicSlider;    // Music Knob/Slider'ı buraya

    void Start()
    {
        // 1. Sahne açıldığında kayıtlı ayarları yükle
        // Eğer kayıt yoksa varsayılan olarak 0.75 (yüksek ses) getirir.
        
        float kayitliMaster = PlayerPrefs.GetFloat("MasterPref", 0.75f);
        float kayitliMusic = PlayerPrefs.GetFloat("MusicPref", 0.75f);

        // 2. Slider/Knob pozisyonlarını güncelle (Görsel senkronizasyon)
        if (masterSlider != null) masterSlider.value = kayitliMaster;
        if (musicSlider != null) musicSlider.value = kayitliMusic;

        // 3. Mixer seslerini güncelle (İşitsel senkronizasyon)
        SetMasterVolume(kayitliMaster);
        SetMusicVolume(kayitliMusic);
    }

    // Master Knob'a bağlanacak fonksiyon
    public void SetMasterVolume(float sliderValue)
    {
        // Slider 0.0001'den küçükse sesi tamamen kapat (Hata önleyici)
        if (sliderValue <= 0.0001f) sliderValue = 0.0001f;

        // Logaritmik dönüştürme: Slider (0-1) -> Desibel (-80, 0)
        float dbValue = Mathf.Log10(sliderValue) * 20;

        audioMixer.SetFloat("MasterVolume", dbValue);
        
        // Ayarı hafızaya kaydet
        PlayerPrefs.SetFloat("MasterPref", sliderValue);
    }

    // Music Knob'a bağlanacak fonksiyon
    public void SetMusicVolume(float sliderValue)
    {
        if (sliderValue <= 0.0001f) sliderValue = 0.0001f;

        float dbValue = Mathf.Log10(sliderValue) * 20;

        audioMixer.SetFloat("MusicVolume", dbValue);
        PlayerPrefs.SetFloat("MusicPref", sliderValue);
    }
}
