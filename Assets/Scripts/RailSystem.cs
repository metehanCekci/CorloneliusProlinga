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
    private float currentGrindSpeed;  // Aktif ray hızı
    
    [Header("Çıkış Ayarları")]
    public float exitBoost = 2f;
    public float exitLaunchAngle = 30f;
    
    private bool isActive = false;
    private ControllerScript player;
    private Transform targetPoint; // Hedef nokta (yöne göre start veya end)
    private Transform originPoint; // Başlangıç nokta
    private Quaternion originalPlayerRotation; // Karakterin orijinal rotasyonu
    private Vector3 originalPlayerScale; // Karakterin orijinal scale'i
    private InputActions railInputActions; // Input referansı
    private InputAction playerMoveAction; // Oyuncunun hareket inputu

    private void Awake() => this.enabled = false;

    private void Update()
    {
        if (!isActive || player == null || targetPoint == null) return;

        // Karakteri hedef noktaya doğru sürükle
        Vector3 direction = (targetPoint.position - player.transform.position).normalized;
        
        // Ray eğimini hesapla (sadece Z rotasyonu)
        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        
        // Yön için sadece X scale'i değiştir, diğerlerini koru
        float scaleX = direction.x >= 0 ? Mathf.Abs(originalPlayerScale.x) : -Mathf.Abs(originalPlayerScale.x);
        player.transform.localScale = new Vector3(scaleX, originalPlayerScale.y, originalPlayerScale.z);
        player.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Sign(scaleX));
        
        if (playerMoveAction != null)
        {
            float moveInput = playerMoveAction.ReadValue<float>();
            
            // Ters yöne basıyorsa yön değiştir
            bool pushingReverse = (moveInput > 0 && direction.x < 0) || (moveInput < 0 && direction.x > 0);
            if (pushingReverse)
            {
                // Target ve origin'i swap et
                Transform temp = targetPoint;
                targetPoint = originPoint;
                originPoint = temp;
                
                // Hızı biraz düşür (yön değiştirme maliyeti)
                currentGrindSpeed = Mathf.Max(currentGrindSpeed * 0.7f, minGrindSpeed);
                return; // Bu frame'de hareket etme, yön değişti
            }
            
            // İleri yöne basıyorsa hızlan (max hıza kadar)
            bool pushingForward = (moveInput > 0 && direction.x > 0) || (moveInput < 0 && direction.x < 0);
            if (pushingForward && currentGrindSpeed < maxGrindSpeed)
            {
                currentGrindSpeed += railAcceleration * Time.deltaTime;
                currentGrindSpeed = Mathf.Min(currentGrindSpeed, maxGrindSpeed);
            }
        }
        
        player.transform.position += direction * currentGrindSpeed * Time.deltaTime;

        // Hedef noktaya vardığında
        if (Vector3.Distance(player.transform.position, targetPoint.position) < 0.2f)
        {
            FinishGrind();
        }
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
            
            // Sadece ray doğal olarak bittiyse target noktasına taşı
            if (teleportToEnd)
            {
                player.transform.position = targetPoint.position + exitDir * 0.5f;
            }
            
            // Serbest atış: Hem ileri hem yukarı fırlat
            float exitSpeed = currentGrindSpeed + exitBoost;
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
}