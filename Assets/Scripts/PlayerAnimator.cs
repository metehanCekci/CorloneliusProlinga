using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Animator animator;
    [SerializeField] private ControllerScript controller;
    [SerializeField] private Gravity gravity;
    [SerializeField] private MaskManager maskManager;
    
    // Animator Parameter Hash'leri (performans için)
    private int speedHash;
    private int isGroundedHash;
    private int isJumpingHash;
    private int isFallingHash;
    private int isDashingHash;
    private int isOnWallHash;
    private int isGrindingHash;
    private int isSkatingHash;
    private int isBrakingHash;
    private int verticalVelocityHash;
    private int jumpHash;
    private int isMaskedHash; // YENİ: Mask durumu
    
    // Cache: Şu anki mask durumu
    private bool currentMaskState = false;
    private bool lastMaskState = false;
    
    void Start()
    {
        // Referansları otomatik bul
        if (animator == null) animator = GetComponent<Animator>();
        if (controller == null) controller = GetComponentInParent<ControllerScript>();
        if (gravity == null) gravity = GetComponentInParent<Gravity>();
        if (maskManager == null) maskManager = MaskManager.Instance; // Singleton kullan
        
        Debug.Log($"PlayerAnimator Start: animator={animator != null}, controller={controller != null}, gravity={gravity != null}, maskManager={maskManager != null}");
        
        if (animator != null)
        {
            Debug.Log($"Animator has controller: {animator.runtimeAnimatorController != null}");
        }
        
        // Hash'leri cache'le (her frame string karşılaştırma yapmamak için)
        speedHash = Animator.StringToHash("Speed");
        isGroundedHash = Animator.StringToHash("isGrounded");
        isJumpingHash = Animator.StringToHash("isJumping");
        isFallingHash = Animator.StringToHash("isFalling");
        isDashingHash = Animator.StringToHash("isDashing");
        isOnWallHash = Animator.StringToHash("isOnWall");
        isGrindingHash = Animator.StringToHash("isGrinding");
        isSkatingHash = Animator.StringToHash("isSkating");
        isBrakingHash = Animator.StringToHash("isBraking");
        verticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        jumpHash = Animator.StringToHash("Jump");
        isMaskedHash = Animator.StringToHash("isMasked"); // YENİ: Hash register et
        
        // Mask değişiklikleri dinle
        if (maskManager != null)
        {
            maskManager.onMaskChanged += OnMaskStateChanged;
        }
        
        // İlk durumu ayarla
        currentMaskState = maskManager != null && maskManager.IsMaskActive();
        lastMaskState = currentMaskState;
    }
    
    /// <summary>
    /// Mask durumu değiştiğinde çağrılır
    /// </summary>
    private void OnMaskStateChanged(bool isNowMasked)
    {
        currentMaskState = isNowMasked;
        Debug.Log($"PlayerAnimator: Mask durumu değişti -> {(isNowMasked ? "Masked" : "Normal")}");

        // Mask tuşuna basıldığında hangi animasyonda olursa olsun anında Idle veya MaskedIdle'a geç
        if (animator != null)
        {
            if (isNowMasked)
            {
                animator.Play("MaskedIdle", 0, 0f);
            }
            else
            {
                animator.Play("Idle", 0, 0f);
            }
        }
    }
    
    public void OnJump()
    {
    UpdateAnimatorParameters();
    Debug.Log($"Jump Trigger: isMasked={animator.GetBool(isMaskedHash)}, isSkating={animator.GetBool(isSkatingHash)}");
    if (animator != null)
    {
        animator.SetTrigger(jumpHash);
    }
    }

    void Update()
    {
        if (animator == null || controller == null || gravity == null)
        {
            Debug.LogWarning($"PlayerAnimator eksik referans! animator={animator != null}, controller={controller != null}, gravity={gravity != null}");
            return;
        }
        
        UpdateAnimatorParameters();
    }
    
    public void UpdateAnimatorParameters()
    {
        Vector3 velocity = gravity.GetVelocity();
        
        // === MASK KONTROLÜ ===
        // Mask durumu değişmişse, idle state'leri güncelle
        if (currentMaskState != lastMaskState)
        {
            animator.SetBool(isMaskedHash, currentMaskState);
            lastMaskState = currentMaskState;
            Debug.Log($"Animator isMasked parameter set to: {currentMaskState}");
            
            // Geçişi sorunsuz yapması için idle'dan başlatıyoruz
            // Animator transition idle -> masked idle ya da idle -> normal idle yapacak
        }
        
        // === MOVEMENT PARAMETRELERI ===
        // Speed - yatay hareket hızı (0-1 arası normalize)
        // Dash sırasında speed'i 0 yap ki yürüme animasyonu oynamasın
        float normalizedSpeed = controller.IsDashing ? 0f : Mathf.Abs(velocity.x) / controller.maxSpeed;
        animator.SetFloat(speedHash, normalizedSpeed);
        
        // Vertical Velocity - dikey hız (blend tree için)
        animator.SetFloat(verticalVelocityHash, velocity.y);
        
        // === DURUM PARAMETRELERI ===
        // Bool durumlar
        animator.SetBool(isGroundedHash, controller.IsGrounded());
        animator.SetBool(isJumpingHash, velocity.y > 0.1f && !controller.IsGrounded());
        animator.SetBool(isFallingHash, velocity.y < -0.1f && !controller.IsGrounded());
        animator.SetBool(isOnWallHash, controller.IsOnWall());
        animator.SetBool(isGrindingHash, controller.isGrinding);
        animator.SetBool(isDashingHash, controller.IsDashing);
        animator.SetBool(isSkatingHash, controller.isSkating);
        animator.SetBool(isBrakingHash, controller.isBraking);
    }
    
    private void OnDestroy()
    {
        // Listener'ı kaldır
        if (maskManager != null)
        {
            maskManager.onMaskChanged -= OnMaskStateChanged;
        }
    }
}