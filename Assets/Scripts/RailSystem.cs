using UnityEngine;
using UnityEngine.InputSystem;

public class RailSystem : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    
    [Header("Hız Ayarları")]
    public float minGrindSpeed = 2f;  // Minimum ray hızı
    public float maxGrindSpeed = 8f;  // Maximum ray hızı
    public float midGrindSpeed = 5f;  // Orta ray hızı
    public float railAcceleration = 3f; // İleri tuşuyla hızlanma oranı
    public float reverseDeceleration = 8f; // Ters yöne basarken yavaşlama hızı
    public float railDashSpeed = 15f; // Rail'de dash hızı
    public float railDashDuration = 0.3f; // Rail dash süresi
    private float currentGrindSpeed;  // Aktif ray hızı
    private bool isReversing = false; // Yön değiştirme sürecinde mi?
    private bool isRailDashing = false; // Rail dash aktif mi?
    private float railDashTimer = 0f;
    
    [Header("Çıkış Ayarları")]
    public float exitBoost = 2f;
    public float exitLaunchAngle = 30f;
    [SerializeField] private LayerMask railLayer; // Diğer railleri tespit için
    [SerializeField] private float chainCheckRadius = 0.5f; // Zincir ray tespiti yarıçapı
    
    private bool isActive = false;
    private ControllerScript player;
    private Transform targetPoint; // Hedef nokta (yöne göre start veya end)
    private Transform originPoint; // Başlangıç nokta
    private Quaternion originalPlayerRotation; // Karakterin orijinal rotasyonu
    private Vector3 originalPlayerScale; // Karakterin orijinal scale'i
    private InputActions railInputActions; // Input referansı
    private InputAction playerMoveAction; // Oyuncunun hareket inputu
    private MaskObject maskObject; // Mask kontrolü için

    private void Awake()
    {
        this.enabled = false;
        maskObject = GetComponent<MaskObject>();
        
        // Rail layer'ı otomatik ata
        if (railLayer == 0) railLayer = LayerMask.GetMask("Rail");
    }
    
    private void OnEnable()
    {
        // Maske değişikliğini dinle
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.onMaskChanged += OnMaskChanged;
        }
    }
    
    private void OnDisable()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.onMaskChanged -= OnMaskChanged;
        }
    }
    
    private void OnMaskChanged(bool isMaskOn)
    {
        // Eğer bu ray bir MaskObject ise ve artık aktif değilse, karakteri çıkar
        if (maskObject != null && isActive)
        {
            bool shouldBeSolid = (maskObject.GetWorldType() == MaskObject.ObjectWorldType.Natural) ? !isMaskOn : isMaskOn;
            if (!shouldBeSolid)
            {
                FinishGrind(false); // Raydan düşür
            }
        }
    }

    private void Update()
    {
        if (!isActive || player == null || targetPoint == null) return;

        // Rail dash timer
        if (isRailDashing)
        {
            railDashTimer -= Time.deltaTime;
            if (railDashTimer <= 0)
            {
                isRailDashing = false;
                currentGrindSpeed = maxGrindSpeed; // Dash bitince max hıza düş
            }
        }

        // Karakteri hedef noktaya doğru sürükle
        Vector3 direction = (targetPoint.position - player.transform.position).normalized;
        
        // Aktif hız (dash varsa dash hızı, yoksa normal)
        float activeSpeed = isRailDashing ? railDashSpeed : currentGrindSpeed;
        
        // Ray eğimini hesapla (sadece Z rotasyonu)
        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        
        // Yön için sadece X scale'i değiştir, diğerlerini koru
        float scaleX = direction.x >= 0 ? Mathf.Abs(originalPlayerScale.x) : -Mathf.Abs(originalPlayerScale.x);
        player.transform.localScale = new Vector3(scaleX, originalPlayerScale.y, originalPlayerScale.z);
        player.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Sign(scaleX));
        
        if (playerMoveAction != null)
        {
            float moveInput = playerMoveAction.ReadValue<float>();
            
            // Ters yöne basıyorsa yavaşça dur ve yön değiştir
            bool pushingReverse = (moveInput > 0 && direction.x < 0) || (moveInput < 0 && direction.x > 0);
            if (pushingReverse)
            {
                isReversing = true;
                // Yavaşça yavaşla
                currentGrindSpeed -= reverseDeceleration * Time.deltaTime;
                
                // Tamamen durduğunda yön değiştir
                if (currentGrindSpeed <= 0.1f)
                {
                    currentGrindSpeed = 0.1f;
                    // Target ve origin'i swap et
                    Transform temp = targetPoint;
                    targetPoint = originPoint;
                    originPoint = temp;
                    isReversing = false;
                }
            }
            else
            {
                isReversing = false;
                
                // İleri yöne basıyorsa hızlan (max hıza kadar)
                bool pushingForward = (moveInput > 0 && direction.x > 0) || (moveInput < 0 && direction.x < 0);
                if (pushingForward && currentGrindSpeed < maxGrindSpeed)
                {
                    currentGrindSpeed += railAcceleration * Time.deltaTime;
                    currentGrindSpeed = Mathf.Min(currentGrindSpeed, maxGrindSpeed);
                }
            }
        }
        
        player.transform.position += direction * activeSpeed * Time.deltaTime;

        // Hedef noktaya vardığında
        if (Vector3.Distance(player.transform.position, targetPoint.position) < 0.2f)
        {
            FinishGrind();
        }
    }
    
    /// <summary>
    /// Rail'de dash - geçici yüksek hız
    /// </summary>
    public void ApplyRailDash()
    {
        isRailDashing = true;
        railDashTimer = railDashDuration;
    }

    public void StartGrindFromManual(ControllerScript targetPlayer, float entrySpeed, float entryDirection)
    {
        if (isActive || targetPlayer == null || startPoint == null || endPoint == null) return;
        
        player = targetPlayer;
        isActive = true;
        
        // Karakterin giriş yönüne göre ray yönünü belirle
        Vector3 railDir = (endPoint.position - startPoint.position).normalized;
        
        // Eğer karakter rayın tersi yönde gidiyorsa, end'den start'a git
        if ((entryDirection < 0 && railDir.x > 0) || (entryDirection > 0 && railDir.x < 0))
        {
            // Ters yön: end'den start'a
            targetPoint = startPoint;
            originPoint = endPoint;
        }
        else
        {
            // Normal yön: start'tan end'e
            targetPoint = endPoint;
            originPoint = startPoint;
        }
        
        // Giriş hızına göre ray hızını belirle
        currentGrindSpeed = Mathf.Clamp(entrySpeed, minGrindSpeed, maxGrindSpeed);
        
        // Karakterin orijinal rotasyonunu ve scale'ini kaydet
        originalPlayerRotation = player.transform.rotation;
        originalPlayerScale = player.transform.localScale;
        
        // Oyuncunun input aksiyonunu al (hızlanma için)
        if (railInputActions == null)
        {
            railInputActions = new InputActions();
        }
        railInputActions.Enable();
        playerMoveAction = railInputActions.Player.Move;
        
        // Karakterin mevcut pozisyonuna göre en yakın noktayı bul
        Vector3 playerPos = player.transform.position;
        Vector3 closestPoint = GetClosestPointOnRail(playerPos);
        player.transform.position = closestPoint;
        
        this.enabled = true; 
        player.EnterRail(currentGrindSpeed, this);
    }
    
    /// <summary>
    /// Zincir geçişi için - pozisyon değiştirmeden direkt rail'e al
    /// </summary>
    public void StartChainGrind(ControllerScript targetPlayer, float speed, float direction, Quaternion origRotation, Vector3 origScale)
    {
        if (isActive || targetPlayer == null || startPoint == null || endPoint == null) return;
        
        player = targetPlayer;
        isActive = true;
        
        // Yönü belirle
        Vector3 railDir = (endPoint.position - startPoint.position).normalized;
        if ((direction < 0 && railDir.x > 0) || (direction > 0 && railDir.x < 0))
        {
            targetPoint = startPoint;
            originPoint = endPoint;
        }
        else
        {
            targetPoint = endPoint;
            originPoint = startPoint;
        }
        
        // Hızı koru (clamp yok - mevcut hızla devam)
        currentGrindSpeed = speed;
        
        // Orijinal değerleri aktar (yeni kaydetme)
        originalPlayerRotation = origRotation;
        originalPlayerScale = origScale;
        
        // Input aksiyonunu al
        if (railInputActions == null)
        {
            railInputActions = new InputActions();
        }
        railInputActions.Enable();
        playerMoveAction = railInputActions.Player.Move;
        
        // POZİSYON DEĞİŞTİRMEDEN devam et - bu zincir geçişi için kritik
        // Karakter olduğu yerde kalır, sadece rail sistemi değişir
        
        this.enabled = true;
        player.activeRail = this; // Sadece referansı güncelle, EnterRail çağırma
    }
    
    // Push ile ray hızını artır
    public void UpdateGrindSpeed(int pushCount)
    {
        if (pushCount >= 10) currentGrindSpeed = maxGrindSpeed;
        else if (pushCount >= 5) currentGrindSpeed = midGrindSpeed;
    }
    
    private Vector3 GetClosestPointOnRail(Vector3 pos)
    {
        Vector3 railDir = (endPoint.position - startPoint.position).normalized;
        Vector3 toPos = pos - startPoint.position;
        float projection = Vector3.Dot(toPos, railDir);
        float railLength = Vector3.Distance(startPoint.position, endPoint.position);
        
        // Projeksiyon değerini ray uzunluğuyla sınırla
        projection = Mathf.Clamp(projection, 0f, railLength);
        
        return startPoint.position + railDir * projection;
    }

    public void FinishGrind(bool teleportToEnd = true)
    {
        if (!isActive) return;
        isActive = false;

        if (player != null)
        {
            // Karakterin rotasyonunu ve scale'ini eski haline getir
            player.transform.rotation = originalPlayerRotation;
            player.transform.localScale = originalPlayerScale;
            
            // Çıkış yönü: origin'den target'a doğru
            Vector3 exitDir = (targetPoint.position - originPoint.position).normalized;
            
            // Sadece ray doğal olarak bittiyse (zıplamadıysa) zincir ray kontrolü yap
            if (teleportToEnd)
            {
                // Bitiş noktasında başka rail var mı kontrol et
                RailSystem nextRail = FindNextRail(targetPoint.position, exitDir);
                if (nextRail != null)
                {
                    // Zincir geçiş - fırlatma yok, direkt diğer raile geç
                    float savedSpeed = currentGrindSpeed;
                    ControllerScript savedPlayer = player;
                    float direction = exitDir.x;
                    
                    // Bu rail'i temizle
                    player = null;
                    targetPoint = null;
                    originPoint = null;
                    playerMoveAction = null;
                    if (railInputActions != null) railInputActions.Disable();
                    this.enabled = false;
                    
                    // Diğer rail'e geç (zincir geçişi - pozisyon değişmez)
                    nextRail.StartChainGrind(savedPlayer, savedSpeed, direction, originalPlayerRotation, originalPlayerScale);
                    return;
                }
                
                // Zincir rail yoksa normal fırlatma
                //player.transform.position = targetPoint.position + exitDir * 0.05f;
            }
            
            // Serbest atış: Sadece mevcut hız ile çık (exit boost yok)
            float exitSpeed = currentGrindSpeed;
            float angleRad = exitLaunchAngle * Mathf.Deg2Rad;
            Vector3 launchVelocity = new Vector3(
                exitDir.x * exitSpeed * Mathf.Cos(angleRad),
                exitSpeed * Mathf.Sin(angleRad),
                0f
            );
            
            player.ExitRail(launchVelocity);
        }

        player = null;
        targetPoint = null;
        originPoint = null;
        playerMoveAction = null;
        if (railInputActions != null) railInputActions.Disable();
        this.enabled = false;
    }
    
    private RailSystem FindNextRail(Vector3 position, Vector3 direction)
    {
        // Bitiş noktası + biraz ileri yönde ray ara
        Vector3 checkPos = position + direction * chainCheckRadius;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, chainCheckRadius, railLayer);
        foreach (var hit in hits)
        {
            RailSystem rail = hit.GetComponent<RailSystem>();
            if (rail != null && rail != this && !rail.isActive)
            {
                return rail;
            }
        }
        return null;
    }
}