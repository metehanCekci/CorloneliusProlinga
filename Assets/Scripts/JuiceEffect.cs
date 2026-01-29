using UnityEngine;

public class JuiceEffect : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float lerpSpeed = 15f;
    
    private Vector3 initialScale;

    [Header("Kuvvetler (Carpan Olarak)")]
    [SerializeField] private Vector3 jumpStretch = new Vector3(0.8f, 1.3f, 1f); 
    [SerializeField] private Vector3 landSquash = new Vector3(1.3f, 0.7f, 1f);  
    [SerializeField] private Vector3 dashStretch = new Vector3(1.5f, 0.6f, 1f); 

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime * lerpSpeed);
    }

    public void ApplyStretch() => transform.localScale = Vector3.Scale(initialScale, jumpStretch);
    public void ApplySquish() => transform.localScale = Vector3.Scale(initialScale, landSquash);
    public void ApplyDashStretch() => transform.localScale = Vector3.Scale(initialScale, dashStretch);
}
