using UnityEngine;

public class PlayerSlopeRotator : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public float rotationSpeed = 15f; 
    public float rayLength = 1.5f;
    public float rayOffsetY = 0.5f;
    public LayerMask groundLayer;

    [Header("Yön Ayarları (İnce Ayar)")]
    [Tooltip("Sağa bakarken (Scale X > 0) açıya eklenecek sapma.")]
    [Range(-20f, 20f)]
    public float sagEgimOffset = 0f;

    [Tooltip("Sola bakarken (Scale X < 0) açıya eklenecek sapma.")]
    [Range(-20f, 20f)]
    public float solEgimOffset = 0f;

    [Header("Fizik Düzeltmeleri")]
    [Tooltip("Sola bakarken açıyı tamamen tersine çevirir (Yokuş yukarı/aşağı karışıyorsa kullan).")]
    public bool solTarafiTersle = true;
    
    [Tooltip("Çok küçük açı değişimlerini yoksayar. Titremeyi azaltır.")]
    public float titremeEngelleme = 1.0f; 

    [Header("Debug")]
    public bool showRays = true;

    private Transform t;
    private ControllerScript controller;
    private Gravity gravity;
    private Quaternion lastTargetRotation;

    void Start()
    {
        t = transform;
        controller = GetComponent<ControllerScript>();
        gravity = GetComponent<Gravity>(); 
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground");
        
        lastTargetRotation = t.rotation;
    }

    // ÖNEMLİ DEĞİŞİKLİK: Titremeyi önlemek için LateUpdate yerine FixedUpdate kullanıyoruz.
    // Çünkü Gravity ve Rigidbody işlemleri burada yapılır. Senkronize olmazlarsa titreme olur.
    void FixedUpdate()
    {
        // Zemin kontrolü
        bool isGrounded = false;
        if (controller != null) isGrounded = controller.IsGrounded();
        else if (gravity != null) isGrounded = gravity.IsGrounded();

        // Yokuş aşağı hızlı inerken temas kopmasın diye ek kontrol
        if (!isGrounded && gravity != null && gravity.GetVelocity().y <= 0.1f)
        {
            isGrounded = Physics2D.Raycast(t.position + Vector3.up * rayOffsetY, Vector2.down, rayLength, groundLayer);
        }

        if (isGrounded)
        {
            
        }
        else
        {
            // Havada yavaşça düzel
            RotateSmoothly(Quaternion.identity);
        }
        AlignToSlope();
    }

    void AlignToSlope()
    {
        Vector3 origin = t.position + Vector3.up * rayOffsetY;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundLayer);

        if (showRays) Debug.DrawRay(origin, Vector3.down * rayLength, hit.collider != null ? Color.green : Color.red);

        if (hit.collider != null)
        {
            // 1. Normalden Açıyı Bul
            Vector2 normal = hit.normal;
            float angle = Vector2.SignedAngle(Vector2.up, normal);

            // --- YÖNE ÖZEL AYARLAR ---
            
            // Karakter SOLA bakıyorsa (Scale X < 0)
            if (t.localScale.x < 0)
            {
                // Sol tarafın mantığını ters çevir (Eğer gerekiyorsa)
                if (solTarafiTersle) angle = -angle;

                // Sola özel ince ayarı ekle
                angle += solEgimOffset;
            }
            // Karakter SAĞA bakıyorsa (Scale X > 0)
            else
            {
                // Sağa özel ince ayarı ekle
                angle += sagEgimOffset;
            }

            // 2. TİTREME ENGELLEME (THRESHOLD)
            // Eğer yeni açı ile şimdiki açı arasındaki fark çok azsa (örn 1 derece), hiç dönme.
            // Bu, milimetrik hesaplamalar yüzünden oluşan titremeyi keser.
            if (Mathf.Abs(angle - t.eulerAngles.z) < titremeEngelleme) return;

            // Rotasyonu Hedefle
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            RotateSmoothly(targetRotation);
        }
    }

    void RotateSmoothly(Quaternion target)
    {
        // FixedUpdate içinde olduğumuz için Time.fixedDeltaTime kullanmalıyız
        t.rotation = Quaternion.Slerp(t.rotation, target, Time.fixedDeltaTime * rotationSpeed);
    }
}
