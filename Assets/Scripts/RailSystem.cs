using UnityEngine;

public class RailSystem : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    
    [Header("Hız Ayarları")]
    public float minGrindSpeed = 2f;  // Minimum ray hızı
    public float maxGrindSpeed = 8f;  // Maximum ray hızı
    public float midGrindSpeed = 5f;  // Orta ray hızı
    private float currentGrindSpeed;  // Aktif ray hızı
    
    [Header("Çıkış Ayarları")]
    public float exitBoost = 2f;
    public float exitLaunchAngle = 30f;
    
    private bool isActive = false;
    private ControllerScript player;
    private Transform targetPoint; // Hedef nokta (yöne göre start veya end)
    private Transform originPoint; // Başlangıç nokta

    private void Awake() => this.enabled = false;

    private void Update()
    {
        if (!isActive || player == null || targetPoint == null) return;

        // Karakteri hedef noktaya doğru sürükle
        Vector3 direction = (targetPoint.position - player.transform.position).normalized;
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
        
        // Karakteri rayın çocuğu yap (fizik müdahalelerini engelle)
        player.transform.SetParent(this.transform);
        
        // Karakterin mevcut pozisyonuna göre en yakın noktayı bul
        Vector3 playerPos = player.transform.position;
        Vector3 closestPoint = GetClosestPointOnRail(playerPos);
        player.transform.position = closestPoint;
        
        this.enabled = true; 
        player.EnterRail(currentGrindSpeed);
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
            // Karakteri raydan ayır
            player.transform.SetParent(null);
            
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
        this.enabled = false;
    }
}