using UnityEngine;
using UnityEngine.Rendering.Universal; // URP Işıkları için şart
using System.Collections;

public class LightManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public Light2D globalLight;

    private Coroutine currentFadeRoutine;

    // Hem rengi hem de şiddeti aynı anda değiştiren fonksiyon
    public void ChangeAtmosphere(float targetIntensity, Color targetColor, float duration)
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(AtmosphereRoutine(targetIntensity, targetColor, duration));
    }

    private IEnumerator AtmosphereRoutine(float targetIntensity, Color targetColor, float duration)
    {
        if (globalLight == null) yield break;

        float startIntensity = globalLight.intensity;
        Color startColor = globalLight.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Zaman ölçeğinden etkilenmemesi için unscaledDeltaTime kullanabiliriz
            // ama oyun içi zamanla uyumlu olsun dersen deltaTime kalsın.
            elapsedTime += Time.deltaTime;
            
            float t = elapsedTime / duration;

            // Şiddet Geçişi
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            
            // Renk Geçişi
            globalLight.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        // Değerleri tam oturt
        globalLight.intensity = targetIntensity;
        globalLight.color = targetColor;
    }
}
