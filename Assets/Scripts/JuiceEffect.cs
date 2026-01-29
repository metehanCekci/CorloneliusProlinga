using UnityEngine;

public class JuiceEffect : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float lerpSpeed = 15f;
    
    // Hafıza: Editörde verdiğin ilk scale değerini burada tutacağız
    private Vector3 initialScale;

    [Header("Kuvvetler (Çarpan Olarak)")]
    // Bunları çarpan olarak kullanırsak her türlü ana scale değerine uyum sağlar
    [SerializeField] private Vector3 jumpStretch = new Vector3(0.8f, 1.3f, 1f); 
    [SerializeField] private Vector3 landSquash = new Vector3(1.3f, 0.7f, 1f);  
    [SerializeField] private Vector3 dashStretch = new Vector3(1.5f, 0.6f, 1f); 

    void Start()
    {
        // Oyun başladığı andaki scale neyse onu kaydet (Örn: 2,2,2)
        initialScale = transform.localScale;
    }

    void Update()
    {
        // Artık Vector3.one yerine senin kendi initialScale değerine döner
        transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime * lerpSpeed);
    }

    // Efektleri uygularken ana scale ile çarparak uygularsak oran bozulmaz
    public void ApplyStretch() => transform.localScale = Vector3.Scale(initialScale, jumpStretch);
    public void ApplySquish() => transform.localScale = Vector3.Scale(initialScale, landSquash);
    public void ApplyDashStretch() => transform.localScale = Vector3.Scale(initialScale, dashStretch);
}