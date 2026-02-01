using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerScript : MonoBehaviour
{
    [Header("SES SİSTEMİ")]
    public SkateSoundManager soundManager;

    [Header("Hız Ayarları")]
    public float walkSpeed = 1f;
    public float midSpeed = 3f;
    public float maxSpeed = 5f;
    public float currentMoveSpeed = 1f;
    [SerializeField] private float accelerationTime = 2f;
    [SerializeField] private float decelerationTime = 0.5f;

    [Header("Durumlar")]
    public bool isGrinding = false;
    public bool isSkating = false;
    public bool isBraking = false;
    public bool HasDash = true;
    public bool HasSecondJump = true;

    private bool wasGrounded;

    [Header("Ray & Momentum")]
    [HideInInspector] public float railCooldown = 0f;
    [HideInInspector] public RailSystem activeRail = null;
    public const float RAIL_COOLDOWN_TIME = 0.5f;
    public float momentumTime = 0f;
    private float storedMoveSpeed;

    // --- YENİ EKLENEN: Rail Coyote Time ---
    [Header("Coyote Time Ayarları")]
    [SerializeField] private float railCoyoteTime = 0.15f; // Raydan çıktıktan sonra zıplamak için tanınan süre
    private float railCoyoteTimer = 0f;

    [Header("Dash Ayarları")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float groundDashCooldown = 1f;
    public bool IsDashing { get; private set; } = false;
    private float dashTimer = 0f;
    private float dashDirection = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 preDashVelocity;

    [Header("Wall Climb Ayarları")]
    [SerializeField] private float wallClimbSpeed = 4f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float maxStamina = 3f;
    [SerializeField] private float staminaRecoveryRate = 1f;
    [SerializeField] private float wallJumpForceX = 8f;
    [SerializeField] private float wallJumpForceY = 10f;
    [SerializeField] private float wallClimbJumpBoost = 3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float staminaWarningThreshold = 0.3f;
    [SerializeField] private bool requireGrabButton = true;

    private float currentStamina;
    private bool isOnWall = false;
    private int wallDirection = 0;
    private bool isGrabbing = false;
    private SpriteRenderer playerSprite;
    private Color originalColor;
    private float staminaFlashTimer = 0f;

    [Header("Görsel Efektler (Juice & Dust)")]
    [SerializeField] private GameObject DustEffectPrefab;
    private JuiceEffect juice;

    private Gravity gravity;
    private InputActions inputActions;
    private InputAction moveAction, jumpAction, pushAction, brakeAction, dashAction, grabAction;
    private Collider2D col;
    private bool hasGrabAction = false;
    private PlayerAnimator playerAnimator;

    void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        pushAction = inputActions.Player.Push;
        brakeAction = inputActions.Player.Brake;
        dashAction = inputActions.Player.Dash;

        try { grabAction = inputActions.Player.Grab; hasGrabAction = true; }
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
        col = GetComponent<Collider2D>();
        currentStamina = maxStamina;
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerSprite != null) originalColor = playerSprite.color;
        if (wallLayer == 0) wallLayer = LayerMask.GetMask("Ground");
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        if (soundManager == null) soundManager = GetComponent<SkateSoundManager>();
    }

    void Update()
    {
        if (railCooldown > 0) railCooldown -= Time.deltaTime;
        if (momentumTime > 0) momentumTime -= Time.deltaTime;
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        // --- Rail Coyote Timer Güncellemesi ---
        if (railCoyoteTimer > 0) railCoyoteTimer -= Time.deltaTime;

        isGrabbing = hasGrabAction && grabAction.ReadValue<float>() > 0.5f;
        bool grounded = IsGrounded();

        // --- SES KONTROLÜ ---
        HandleSkateSound(grounded);

        if (grounded && !isOnWall)
        {
            currentStamina = Mathf.MoveTowards(currentStamina, maxStamina, staminaRecoveryRate * Time.deltaTime);
            ResetStaminaWarning();
        }

        UpdateStaminaWarning();
        CheckWallState();

        if (IsDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                IsDashing = false;
                currentMoveSpeed = storedMoveSpeed;
                Vector3 post = gravity.GetVelocity();
                post.x = preDashVelocity.x;
                gravity.SetVelocity(post);
                if (IsGrounded()) HasDash = true;
            }
        }

        if (isGrinding) return;

        if (isOnWall)
        {
            HandleWallClimb();
            if (soundManager != null) soundManager.StopLoop();
            return;
        }

        ApplyMovement();
    }

    public void OnLand(float impactVelocity)
    {
        HasDash = true;
        HasSecondJump = true;
        if (juice != null) juice.ApplySquish();

        if (soundManager != null && impactVelocity < -1f)
        {
            soundManager.PlayLand();
        }
    }

    private void HandleSkateSound(bool grounded)
    {
        if (soundManager == null) return;

        if (isGrinding)
        {
            soundManager.StartRailGrind();
            return;
        }

        if (isBraking && grounded)
        {
            soundManager.StartBraking();
            return;
        }

        if (!grounded || isOnWall)
        {
            soundManager.StopLoop();
            return;
        }

        float absSpeed = Mathf.Abs(gravity.GetVelocity().x);
        if (absSpeed > 0.1f)
        {
            soundManager.StartSkating(absSpeed);
        }
        else
        {
            soundManager.StopLoop();
        }
    }

    private void CheckWallState()
    {
        if (isGrinding || IsDashing) { if (isOnWall) ExitWall(); return; }
        if (requireGrabButton) { if (!isGrabbing) { if (isOnWall) ExitWall(); return; } }
        if (currentStamina <= 0) { if (isOnWall) ExitWall(); return; }

        int detectedWall = DetectWall();
        if (detectedWall != 0) { if (!isOnWall) EnterWall(detectedWall); }
        else { if (isOnWall) ExitWall(); }
    }

    private int DetectWall()
    {
        if (col == null) return 0;
        Vector2 rightOrigin = new Vector2(col.bounds.max.x, col.bounds.center.y);
        Vector2 leftOrigin = new Vector2(col.bounds.min.x, col.bounds.center.y);

        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.left, wallCheckDistance, wallLayer);

        if (requireGrabButton && isGrabbing)
        {
            if (rightHit.collider != null) return 1;
            if (leftHit.collider != null) return -1;
            return 0;
        }

        float moveDir = moveAction.ReadValue<float>();
        if (rightHit.collider != null && moveDir > 0) return 1;
        if (leftHit.collider != null && moveDir < 0) return -1;
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
        Vector3 v = gravity.GetVelocity();
        v.y = 0; v.x = 0;
        gravity.SetVelocity(v);
        if (soundManager != null) soundManager.StopLoop();
    }

    private void ExitWall() { isOnWall = false; wallDirection = 0; }

    private void HandleWallClimb()
    {
        float verticalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) verticalInput = 1f;
            else if (Keyboard.current.sKey.isPressed) verticalInput = -1f;
        }

        if (verticalInput <= 0 && IsGrounded()) { ExitWall(); return; }

        if (verticalInput > 0)
        {
            int detectedWall = DetectWall();
            if (detectedWall == 0)
            {
                ExitWall();
                Vector3 exitVel = new Vector3(wallDirection * 3f, 6f, 0);
                gravity.SetVelocity(exitVel);
                return;
            }
        }

        Vector3 v = Vector3.zero;
        if (verticalInput > 0 && currentStamina > 0) { v.y = wallClimbSpeed; currentStamina -= Time.deltaTime; }
        else if (verticalInput < 0) { v.y = -wallClimbSpeed; }
        else { v.y = -wallSlideSpeed; currentStamina -= Time.deltaTime * 0.5f; }

        gravity.SetVelocity(v);
        transform.localScale = new Vector3(-wallDirection, 1, 1);
    }

    public bool IsOnWall() => isOnWall;
    public int GetWallDirection() => wallDirection;
    public float GetStaminaPercent() => currentStamina / maxStamina;

    private void UpdateStaminaWarning()
    {
        if (playerSprite == null) return;
        float staminaPercent = currentStamina / maxStamina;
        if (isOnWall && staminaPercent < staminaWarningThreshold)
        {
            staminaFlashTimer += Time.deltaTime * 10f;
            float flash = (Mathf.Sin(staminaFlashTimer) + 1f) / 2f;
            playerSprite.color = Color.Lerp(Color.red, originalColor, flash);
        }
        else { ResetStaminaWarning(); }
    }

    private void ResetStaminaWarning() { if (playerSprite != null) playerSprite.color = originalColor; staminaFlashTimer = 0f; }
    public bool IsGrounded() => gravity != null && gravity.IsGrounded();

    private void OnJumpStart(InputAction.CallbackContext context)
    {
        if (isOnWall) { PerformWallJump(); if (soundManager != null) soundManager.PlayOllie(); return; }

        // --- GÜNCELLENEN KISIM: Coyote Time Kontrolü ---
        // Eğer grind yapıyorsak VEYA coyote time içindeysek zıpla
        if (isGrinding || railCoyoteTimer > 0)
        {
            // Coyote time'ı tüket
            railCoyoteTimer = 0f;

            // Eğer hala grind üzerindeysek (manuel zıplama) raydan kopar
            if (isGrinding && activeRail != null)
            {
                activeRail.FinishGrind(false);
            }
            // Değilsek zaten fiziksel olarak raydan çıkmışızdır (Coyote Time işliyor),
            // sadece zıplama kuvvetini uygula.

            gravity.StartJump();
            juice?.ApplyStretch();
            SpawnDust();
            playerAnimator?.OnJump();
            if (soundManager != null) soundManager.PlayOllie();
            return;
        }

        // Normal zemin veya Double Jump
        if (IsGrounded() || HasSecondJump)
        {
            if (!IsGrounded()) HasSecondJump = false;
            gravity.StartJump(); juice?.ApplyStretch(); SpawnDust(); playerAnimator?.OnJump();
            if (soundManager != null) soundManager.PlayOllie();
        }
    }

    private void PerformWallJump()
    {
        float extraYBoost = 0f;
        if (Keyboard.current != null && Keyboard.current.wKey.isPressed) extraYBoost = wallClimbJumpBoost;

        int savedWallDir = wallDirection;
        float horizontalInput = moveAction.ReadValue<float>();
        float jumpXForce = wallJumpForceX;
        float jumpYForce = wallJumpForceY + extraYBoost;

        int inputDir = 0;
        if (horizontalInput > 0.3f) inputDir = 1; else if (horizontalInput < -0.3f) inputDir = -1;

        bool isOppositeDirection = false;
        bool isSameDirection = false;

        if (requireGrabButton)
        {
            isOppositeDirection = (inputDir != 0 && inputDir == -savedWallDir);
            isSameDirection = (inputDir != 0 && inputDir == savedWallDir);
        }
        else
        {
            isOppositeDirection = (inputDir == -savedWallDir);
            isSameDirection = (inputDir == 0);
        }

        if (isOppositeDirection) { jumpXForce = wallJumpForceX * 1.5f; jumpYForce = wallJumpForceY * 0.85f + extraYBoost; }
        else if (isSameDirection) { jumpXForce = wallJumpForceX * 0.2f; jumpYForce = wallJumpForceY * 1.3f + extraYBoost; }

        ExitWall();
        Vector3 jumpVel = new Vector3(-savedWallDir * jumpXForce, jumpYForce, 0);
        gravity.SetVelocity(jumpVel);
        transform.localScale = new Vector3(-savedWallDir, 1, 1);
        juice?.ApplyStretch(); SpawnDust();
        StartCoroutine(WallJumpCooldown());
    }

    private System.Collections.IEnumerator WallJumpCooldown()
    {
        float originalWallCheckDist = wallCheckDistance; wallCheckDistance = 0f;
        yield return new WaitForSeconds(0.15f); wallCheckDistance = originalWallCheckDist;
    }

    private void SpawnDust()
    {
        if (DustEffectPrefab != null)
        {
            GameObject clone = Instantiate(DustEffectPrefab);
            clone.transform.position = DustEffectPrefab.transform.position;
            clone.transform.localScale = DustEffectPrefab.transform.lossyScale;
            clone.SetActive(true); Destroy(clone, 2f);
        }
    }

    private void OnJumpCancel(InputAction.CallbackContext context) => gravity?.EndJump();
    private void OnBrake(InputAction.CallbackContext context) => ResetSpeed();

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!HasDash || IsDashing || dashCooldownTimer > 0) return;
        if (isGrinding && activeRail != null)
        {
            HasDash = false; activeRail.ApplyRailDash(); juice?.ApplyDashStretch();
            if (soundManager != null) soundManager.PlayOllie(); return;
        }

        bool wasGrounded = IsGrounded();
        HasDash = false; IsDashing = true; dashTimer = dashDuration;
        storedMoveSpeed = currentMoveSpeed; preDashVelocity = gravity.GetVelocity();
        momentumTime = 0f;
        if (wasGrounded) dashCooldownTimer = groundDashCooldown;

        float input = moveAction.ReadValue<float>();
        dashDirection = input != 0 ? Mathf.Sign(input) : (transform.localScale.x >= 0 ? 1f : -1f);
        juice?.ApplyDashStretch();

        // --- YENİ EKLENEN: Dash Sesi ---
        if (soundManager != null) soundManager.PlayDash();
    }

    public void ResetSpeed() { currentMoveSpeed = walkSpeed; }
    public void ResetDash() { IsDashing = false; dashTimer = 0f; HasDash = true; HasSecondJump = true; }
    public bool CanEnterRail() => railCooldown <= 0 && !isGrinding;

    public void EnterRail(float entrySpeed, RailSystem rail)
    {
        isGrinding = true; activeRail = rail; HasDash = true; HasSecondJump = true; juice?.ApplySquish();
        // Grind'a girince coyote time'ı sıfırla
        railCoyoteTimer = 0f;
        if (soundManager != null) soundManager.StartRailGrind();
    }

    public void ExitRail(Vector3 vel)
    {
        isGrinding = false;
        activeRail = null;
        railCooldown = RAIL_COOLDOWN_TIME;
        momentumTime = 0f;

        // --- GÜNCELLENEN KISIM: Coyote Time Başlat ---
        // Raydan çıktığımız an sayacı başlatıyoruz.
        railCoyoteTimer = railCoyoteTime;

        if (Mathf.Abs(vel.x) > 0.1f) transform.localScale = new Vector3(Mathf.Sign(vel.x), 1, 1);
        if (gravity != null) gravity.SetVelocity(vel);
        if (soundManager != null) soundManager.StopLoop();
    }

    private void ApplyMovement()
    {
        if (IsDashing)
        {
            isBraking = false;
            float dashDistance = dashSpeed * Time.deltaTime;
            float xEdge = dashDirection > 0 ? col.bounds.max.x : col.bounds.min.x;
            Vector2 topOrigin = new Vector2(xEdge, col.bounds.max.y - 0.05f);
            Vector2 midOrigin = new Vector2(xEdge, col.bounds.center.y);
            Vector2 botOrigin = new Vector2(xEdge, col.bounds.min.y + 0.05f);

            RaycastHit2D hit = Physics2D.Raycast(topOrigin, Vector2.right * dashDirection, dashDistance + 0.1f, wallLayer);
            if (hit.collider == null) hit = Physics2D.Raycast(midOrigin, Vector2.right * dashDirection, dashDistance + 0.1f, wallLayer);
            if (hit.collider == null) hit = Physics2D.Raycast(botOrigin, Vector2.right * dashDirection, dashDistance + 0.1f, wallLayer);

            if (hit.collider != null)
            {
                float stopX = hit.point.x - (dashDirection > 0 ? col.bounds.extents.x : -col.bounds.extents.x);
                transform.position = new Vector3(stopX, transform.position.y, transform.position.z);
                IsDashing = false; currentMoveSpeed = storedMoveSpeed;
                Vector3 stopVel = gravity.GetVelocity(); stopVel.x = 0; gravity.SetVelocity(stopVel);
                return;
            }

            Vector3 dashVel = gravity.GetVelocity(); dashVel.x = dashDirection * dashSpeed; dashVel.y = 0;
            gravity.SetVelocity(dashVel); return;
        }

        float dir = moveAction.ReadValue<float>();
        Vector3 v = gravity.GetVelocity();
        bool isTryingToStop = Mathf.Approximately(dir, 0f) && Mathf.Abs(v.x) > 0.1f;
        bool isReversingDirection = isSkating && dir != 0f && Mathf.Sign(v.x) != Mathf.Sign(dir);

        isBraking = isSkating && (isTryingToStop || isReversingDirection);

        if (Mathf.Abs(v.x) > 0.1f) transform.localScale = new Vector3(Mathf.Sign(v.x), 1, 1);

        if (momentumTime > 0)
        {
            if (dir != 0) v.x = Mathf.Lerp(v.x, dir * currentMoveSpeed, 0.1f);
        }
        else
        {
            float accelRate = (maxSpeed - walkSpeed) / Mathf.Max(0.0001f, accelerationTime);
            float decelRate = (maxSpeed - walkSpeed) / Mathf.Max(0.0001f, decelerationTime);
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, dir != 0 ? maxSpeed : walkSpeed, (dir != 0 ? accelRate : decelRate) * Time.deltaTime);
            isSkating = currentMoveSpeed > 1f;
            float targetVelX = dir * currentMoveSpeed;

            if (Mathf.Approximately(dir, 0f)) v.x = Mathf.MoveTowards(v.x, 0f, decelRate * Time.deltaTime);
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