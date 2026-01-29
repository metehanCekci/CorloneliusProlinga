using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Animator animator;
    [SerializeField] private ControllerScript controller;
    [SerializeField] private Gravity gravity;
    
    // Animator Parameter Hash'leri (performans için)
    private int speedHash;
    private int isGroundedHash;
    private int isJumpingHash;
    private int isFallingHash;
    private int isDashingHash;
    private int isOnWallHash;
    private int isGrindingHash;
    private int verticalVelocityHash;
    
    void Start()
    {
        // Referansları otomatik bul
        if (animator == null) animator = GetComponent<Animator>();
        if (controller == null) controller = GetComponentInParent<ControllerScript>();
        if (gravity == null) gravity = GetComponentInParent<Gravity>();
        
        Debug.Log($"PlayerAnimator Start: animator={animator != null}, controller={controller != null}, gravity={gravity != null}");
        
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
        verticalVelocityHash = Animator.StringToHash("VerticalVelocity");
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
    
    private void UpdateAnimatorParameters()
    {
        Vector3 velocity = gravity.GetVelocity();
        
        // Speed - yatay hareket hızı (0-1 arası normalize)
        float normalizedSpeed = Mathf.Abs(velocity.x) / controller.maxSpeed;
        animator.SetFloat(speedHash, normalizedSpeed);
        
        // DEBUG
        Debug.Log($"Speed: {normalizedSpeed:F2}, Grounded: {controller.IsGrounded()}, VelX: {velocity.x:F2}");
        
        // Vertical Velocity - dikey hız (blend tree için)
        animator.SetFloat(verticalVelocityHash, velocity.y);
        
        // Bool durumlar
        animator.SetBool(isGroundedHash, controller.IsGrounded());
        animator.SetBool(isJumpingHash, velocity.y > 0.1f && !controller.IsGrounded());
        animator.SetBool(isFallingHash, velocity.y < -0.1f && !controller.IsGrounded());
        animator.SetBool(isOnWallHash, controller.IsOnWall());
        animator.SetBool(isGrindingHash, controller.isGrinding);
        animator.SetBool(isDashingHash, controller.IsDashing);
    }
}
