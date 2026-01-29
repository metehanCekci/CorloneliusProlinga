using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerScript : MonoBehaviour
{
    [Header("Hız Ayarları")]
    public float walkSpeed = 1f;
    public float midSpeed = 3f;
    public float maxSpeed = 5f;
    public float currentMoveSpeed = 1f;
    [SerializeField] private float accelerationTime = 2f; 
    [SerializeField] private float decelerationTime = 0.5f; 

    [Header("Durumlar")]
    public bool isGrinding = false;
    public bool HasDash = true;
    public bool HasSecondJump = true;
    private bool wasGrounded;

    [Header("Ray & Momentum")]
    [HideInInspector] public float railCooldown = 0f;
    [HideInInspector] public RailSystem activeRail = null;
    private const float RAIL_COOLDOWN_TIME = 0.5f;
    private float momentumTime = 0f; 
    private float storedMoveSpeed;

    [Header("Dash Ayarları")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float groundDashCooldown = 1f; // Yerde dash cooldown
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashDirection = 0f;
    private float dashCooldownTimer = 0f;

    [Header("Wall Climb Ayarları")]
    [SerializeField] private float wallClimbSpeed = 4f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float maxStamina = 3f; // Tırmanma süresi (saniye)
    [SerializeField] private float staminaRecoveryRate = 1f; // Yerde iken saniyede dolum
    [SerializeField] private float wallJumpForceX = 8f;
    [SerializeField] private float wallJumpForceY = 10f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.1f;
    
    // Celeste modu için: true = tuşa basılı tutmak gerekir, false = otomatik tutunma
    [SerializeField] private bool requireGrabButton = true;
    
    private float currentStamina;
    private bool isOnWall = false;
    private int wallDirection = 0; // -1 = sol duvar, 1 = sağ duvar
    private bool isGrabbing = false;

    [Header("Görsel Efektler (Juice & Dust)")]
    [SerializeField] private GameObject DustEffectPrefab;
    private JuiceEffect juice;

    private Gravity gravity;
    private InputActions inputActions;
    private InputAction moveAction, jumpAction, pushAction, brakeAction, dashAction, grabAction;
    private Collider2D col; // BoxCollider2D veya CapsuleCollider2D
    private bool hasGrabAction = false;

    void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        pushAction = inputActions.Player.Push;
        brakeAction = inputActions.Player.Brake;
        dashAction = inputActions.Player.Dash;
        
        // Grab action opsiyonel - InputActions'da varsa kullan
        try 
        { 
            grabAction = inputActions.Player.Grab; 
            hasGrabAction = true;
        } 
        catch { hasGrabAction = false; }

        brakeAction.performed += OnBrake;
        jumpAction.performed += OnJumpStart;
        jumpAction.canceled += OnJumpCancel;
        dashAction.performed += OnDash;
    }

    void Start()
    {
        gravity = GetComponent<Gravity>();
        juice = GetComponentInChildren<JuiceEffect>();
        col = GetComponent<Collider2D>(); // Her türlü 2D collider çalışır
        currentStamina = maxStamina;
        
        if (wallLayer == 0) wallLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (railCooldown > 0) railCooldown -= Time.deltaTime;
        if (momentumTime > 0) momentumTime -= Time.deltaTime;
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        // Grab input kontrolü (Celeste modu için)
        isGrabbing = hasGrabAction && grabAction.ReadValue<float>() > 0.5f;

        // Yer Kontrolü ve Yere İnme (Squish) Tetikleyici
        bool grounded = IsGrounded();
        if (grounded && !wasGrounded)
        {
            HasDash = true; 
            HasSecondJump = true;
            juice?.ApplySquish();
        }
        wasGrounded = grounded;

        // Stamina Recovery (yerdeyken)
        if (grounded && !isOnWall)
        {
            currentStamina = Mathf.MoveTowards(currentStamina, maxStamina, staminaRecoveryRate * Time.deltaTime);
        }

        // Wall Check
        CheckWallState();

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                currentMoveSpeed = storedMoveSpeed;
                Vector3 post = gravity.GetVelocity();
                float dir = moveAction.ReadValue<float>();
                post.x = (dir != 0) ? dir * currentMoveSpeed : 0f;
                gravity.SetVelocity(post);
                
                // Dash bitti ve yerdeysen dash'i geri ver
                if (IsGrounded())
                {
                    HasDash = true;
                }
            }
        }

        if (isGrinding) return;
        
        // Wall climb kontrolü (yerdeyken de çalışır)
        if (isOnWall)
        {
            HandleWallClimb();
            return;
        }
        
        ApplyMovement();
    }

    private void CheckWallState()
    {
        // Rail veya dash sırasında wall climb yok
        if (isGrinding || isDashing)
        {
            if (isOnWall) ExitWall();
            return;
        }

        // Celeste modu: grab tuşuna basılı tutmak gerekiyor
        if (requireGrabButton)
        {
            if (!isGrabbing)
            {
                if (isOnWall) ExitWall();
                return;
            }
        }

        // Stamina kontrolü
        if (currentStamina <= 0)
        {
            if (isOnWall) ExitWall();
            return;
        }

        int detectedWall = DetectWall();
        
        if (detectedWall != 0)
        {
            if (!isOnWall)
            {
                EnterWall(detectedWall);
            }
        }
        else
        {
            if (isOnWall) ExitWall();
        }
    }

    private int DetectWall()
    {
        if (col == null) return 0;
        
        Vector2 rightOrigin = new Vector2(col.bounds.max.x, col.bounds.center.y);
        Vector2 leftOrigin = new Vector2(col.bounds.min.x, col.bounds.center.y);
        
        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.left, wallCheckDistance, wallLayer);
        
        // Celeste modu: grab tuşuna basıldıysa yöne bakmadan duvara tutun
        if (requireGrabButton && isGrabbing)
        {
            if (rightHit.collider != null) return 1;
            if (leftHit.collider != null) return -1;
            return 0;
        }
        
        float moveDir = moveAction.ReadValue<float>();
        
        // Normal mod: Hareket yönündeki duvara öncelik ver
        if (rightHit.collider != null && moveDir > 0) return 1;
        if (leftHit.collider != null && moveDir < 0) return -1;
        
        // Hareket input yoksa her iki duvara da tutunabilir
        if (moveDir == 0)
        {
            if (rightHit.collider != null) return 1;
            if (leftHit.collider != null) return -1;
        }
        
        return 0;
    }

    private void EnterWall(int direction)
    {
        isOnWall = true;
        wallDirection = direction;
        HasDash = true;
        HasSecondJump = true;
        
        // Duvara tutunduğunda Y hızını sıfırla/yavaşlat
        Vector3 v = gravity.GetVelocity();
        v.y = 0;
        v.x = 0;
        gravity.SetVelocity(v);
    }

    private void ExitWall()
    {
        isOnWall = false;
        wallDirection = 0;
    }

    private void HandleWallClimb()
    {
        float verticalInput = 0f;
        
        // W/S veya Gamepad ile yukarı/aşağı
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) verticalInput = 1f;
            else if (Keyboard.current.sKey.isPressed) verticalInput = -1f;
        }
        
        Vector3 v = gravity.GetVelocity();
        v.x = 0; // Duvardayken X hareketi yok
        
        if (verticalInput > 0 && currentStamina > 0)
        {
            // Yukarı tırmanma
            v.y = wallClimbSpeed;
            currentStamina -= Time.deltaTime;
        }
        else if (verticalInput < 0)
        {
            // Aşağı kayma (stamina harcamaz)
            v.y = -wallClimbSpeed;
        }
        else
        {
            // Duvarda tutunma - yavaş kayma
            v.y = -wallSlideSpeed;
            currentStamina -= Time.deltaTime * 0.5f; // Tutunma yarı stamina harcar
        }
        
        gravity.SetVelocity(v);
        gravity.ApplyWallMovement(); // Pozisyonu güncelle
        
        // Sprite yönü
        transform.localScale = new Vector3(-wallDirection, 1, 1);
    }

    public bool IsOnWall() => isOnWall;
    public int GetWallDirection() => wallDirection;
    public float GetStaminaPercent() => currentStamina / maxStamina;

    public bool IsGrounded() => gravity != null && gravity.IsGrounded();

    private void OnJumpStart(InputAction.CallbackContext context)
    {
        // Wall Jump
        if (isOnWall)
        {
            PerformWallJump();
            return;
        }
        
        if (isGrinding)
        {
            if (activeRail != null) activeRail.FinishGrind(false);
            gravity.StartJump();
            juice?.ApplyStretch();
            SpawnDust();
            return;
        }

        if (IsGrounded() || HasSecondJump)
        {
            if (!IsGrounded()) HasSecondJump = false;
            gravity.StartJump();
            juice?.ApplyStretch();
            SpawnDust();
        }
    }

    private void PerformWallJump()
    {
        ExitWall();
        
        // Duvardan zıt yöne fırlat
        Vector3 jumpVel = new Vector3(
            -wallDirection * wallJumpForceX,
            wallJumpForceY,
            0
        );
        gravity.SetVelocity(jumpVel);
        
        // Sprite yönünü değiştir
        transform.localScale = new Vector3(wallDirection, 1, 1);
        
        juice?.ApplyStretch();
        SpawnDust();
        
        // Wall jump sonrası kısa süre duvar kontrolü yapma
        StartCoroutine(WallJumpCooldown());
    }

    private System.Collections.IEnumerator WallJumpCooldown()
    {
        float originalWallCheckDist = wallCheckDistance;
        wallCheckDistance = 0f; // Geçici olarak duvar algılamayı kapat
        yield return new WaitForSeconds(0.15f);
        wallCheckDistance = originalWallCheckDist;
    }

    private void SpawnDust()
    {
        if (DustEffectPrefab != null)
        {
            GameObject clone = Instantiate(DustEffectPrefab);
            clone.transform.position = DustEffectPrefab.transform.position;
            clone.transform.localScale = DustEffectPrefab.transform.lossyScale;
            clone.SetActive(true);
            Destroy(clone, 2f); // Belleği temizle
        }
    }

    private void OnJumpCancel(InputAction.CallbackContext context) => gravity?.EndJump();
    private void OnBrake(InputAction.CallbackContext context) => ResetSpeed();

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!HasDash || isDashing || dashCooldownTimer > 0) return;
        
        // Raildeyken dash
        if (isGrinding && activeRail != null)
        {
            HasDash = false;
            activeRail.ApplyRailDash();
            juice?.ApplyDashStretch();
            return;
        }
        
        // Yerde dash atıyorsan cooldown başlat
        bool wasGrounded = IsGrounded();
        
        // Normal dash
        HasDash = false;
        isDashing = true;
        dashTimer = dashDuration;
        storedMoveSpeed = currentMoveSpeed;
        
        // Yerde dash attıysan cooldown koy
        if (wasGrounded)
        {
            dashCooldownTimer = groundDashCooldown;
        }

        float input = moveAction.ReadValue<float>();
        dashDirection = input != 0 ? Mathf.Sign(input) : (transform.localScale.x >= 0 ? 1f : -1f);
        juice?.ApplyDashStretch();
    }

    public void ResetSpeed() { currentMoveSpeed = walkSpeed; }
    public bool CanEnterRail() => railCooldown <= 0 && !isGrinding;

    public void EnterRail(float entrySpeed, RailSystem rail)
    {
        isGrinding = true;
        activeRail = rail;
        HasDash = true;
        HasSecondJump = true;
        juice?.ApplySquish(); // Rail'e iniş efekti
    }

    public void ExitRail(Vector3 vel)
    {
        isGrinding = false;
        activeRail = null;
        railCooldown = RAIL_COOLDOWN_TIME;
        momentumTime = 0.3f;
        if (gravity != null) gravity.SetVelocity(vel);
    }

    private void ApplyMovement()
    {
        if (isDashing)
        {
            Vector3 dashVel = gravity.GetVelocity();
            dashVel.x = dashDirection * dashSpeed;
            dashVel.y = 0; 
            gravity.SetVelocity(dashVel);
            return; 
        }

        float dir = moveAction.ReadValue<float>();
        Vector3 v = gravity.GetVelocity();

        if (dir != 0) transform.localScale = new Vector3(Mathf.Sign(dir), 1, 1);

        if (momentumTime > 0)
        {
            if (dir != 0) v.x = Mathf.Lerp(v.x, dir * currentMoveSpeed, 0.1f);
        }
        else
        {
            float accelRate = (maxSpeed - walkSpeed) / Mathf.Max(0.0001f, accelerationTime);
            float decelRate = (maxSpeed - walkSpeed) / Mathf.Max(0.0001f, decelerationTime);

            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, dir != 0 ? maxSpeed : walkSpeed, 
                (dir != 0 ? accelRate : decelRate) * Time.deltaTime);

            float targetVelX = dir * currentMoveSpeed;

            if (Mathf.Approximately(dir, 0f))
            {
                v.x = Mathf.MoveTowards(v.x, 0f, decelRate * Time.deltaTime);
            }
            else
            {
                float rate = (Mathf.Sign(v.x) != Mathf.Sign(dir) && v.x != 0) ? decelRate * 2f : accelRate;
                v.x = Mathf.MoveTowards(v.x, targetVelX, rate * Time.deltaTime);
            }
        }
        gravity.SetVelocity(v);
    }

    void OnDisable() => inputActions.Disable();
}