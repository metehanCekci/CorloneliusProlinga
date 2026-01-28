using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerScript : MonoBehaviour
{
    [Header("Hız Ayarları")]
    public float walkSpeed = 1f, midSpeed = 3f, maxSpeed = 5f, currentMoveSpeed = 1f;
    [SerializeField] private float accelerationTime = 2f; // Max hıza ulaşma süresi
    [SerializeField] private float decelerationRate = 3f; // Durunca yavaşlama hızı
    
    public bool isGrinding = false, HasDash = true, HasSecondJump = true;
    public int railPushCount = 0; // Ray için push sayacı
    private float moveTime = 0f; // Yürüyüş süresi (otomatik hızlanma için)
    
    [HideInInspector] public float railCooldown = 0f;
    private const float RAIL_COOLDOWN_TIME = 0.5f;
    private float momentumTime = 0f; // Fırlatma sonrası momentum koruma süresi

    private Gravity gravity;
    private InputActions inputActions;
    private InputAction moveAction, jumpAction, pushAction, brakeAction, dashAction;
    
    [Header("Dash Ayarları")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashDirection = 0f;

    void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        pushAction = inputActions.Player.Push; 
        brakeAction = inputActions.Player.Brake;
        dashAction = inputActions.Player.Dash;

        pushAction.performed += OnPush;
        brakeAction.performed += OnBrake;
        jumpAction.performed += OnJumpStart;
        dashAction.performed += OnDash;
    }

    void Start() => gravity = GetComponent<Gravity>();

    void Update()
    {
        if (railCooldown > 0) railCooldown -= Time.deltaTime;
        if (momentumTime > 0) momentumTime -= Time.deltaTime;
        
        // Dash timer
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) isDashing = false;
        }
        
        if (isGrinding) return; // Raydayken hiçbir fizik veya hareket çalışmaz
        if (IsGrounded()) { HasDash = true; HasSecondJump = true; }
        ApplyMovement();
    }

    public bool IsGrounded() => gravity != null && gravity.IsGrounded();

    private void OnJumpStart(InputAction.CallbackContext context)
    {
        if (isGrinding) 
        { 
            // Raydayken zıplarsan rayı hemen bitir (teleport yok, olduğun yerden zıpla)
            RailSystem activeRail = GetComponentInParent<RailSystem>();
            if (activeRail != null) activeRail.FinishGrind(false);
            
            // Yerçekimini uyandır ve fırlat
            gravity.StartJump(); 
            return; 
        }
        
        // Yerdeysen normal zıpla
        if (IsGrounded()) 
        {
            gravity.StartJump();
        }
        // Havadaysan ve double jump hakkın varsa
        else if (HasSecondJump)
        {
            HasSecondJump = false;
            gravity.StartJump();
        }
    }

    private void OnPush(InputAction.CallbackContext context)
    {
        // Push sadece raydayken çalışır
        if (!isGrinding) return;
        
        railPushCount++;
        // RailSystem'e hız güncellemesi gönder
        RailSystem activeRail = GetComponentInParent<RailSystem>();
        if (activeRail != null)
        {
            activeRail.UpdateGrindSpeed(railPushCount);
        }
    }

    private void OnBrake(InputAction.CallbackContext context) => ResetSpeed();
    
    private void OnDash(InputAction.CallbackContext context)
    {
        if (!HasDash) return;
        if (isGrinding) return; // Raydayken dash yapamaz (isteğe bağlı değiştirilebilir)
        
        HasDash = false;
        isDashing = true;
        dashTimer = dashDuration;
        
        // Dash yönünü belirle (input varsa o yöne, yoksa baktığı yöne)
        float input = moveAction.ReadValue<float>();
        dashDirection = input != 0 ? Mathf.Sign(input) : (transform.localScale.x >= 0 ? 1f : -1f);
    }

    public void ResetSpeed() { moveTime = 0f; currentMoveSpeed = walkSpeed; }
    public bool CanEnterRail() => railCooldown <= 0 && !isGrinding;
    public void EnterRail(float entrySpeed) 
    { 
        isGrinding = true;
        railPushCount = 0;
        // Rail'e binince dash ve double jump yenilenir (Celeste tarzı)
        HasDash = true;
        HasSecondJump = true;
    }
    
    public void ExitRail(Vector3 vel) 
    { 
        isGrinding = false;
        railCooldown = RAIL_COOLDOWN_TIME;
        momentumTime = 0.3f; // 0.3 saniye momentum koru
        if(gravity != null) gravity.SetVelocity(vel); 
    }

    public float GetCurrentMoveSpeed() => currentMoveSpeed;

    private void ApplyMovement()
    {
        float dir = moveAction.ReadValue<float>();
        Vector3 v = gravity.GetVelocity();
        
        if (isDashing)
        {
            v.x = dashDirection * dashSpeed;
            v.y = 0; // Dash sırasında düşme yok
        }
        else if (momentumTime > 0)
        {
            // Momentum süresinde sadece input varsa biraz etki et, yoksa dokunma
            if (dir != 0) v.x = Mathf.Lerp(v.x, dir * currentMoveSpeed, 0.1f);
        }
        else
        {
            // Otomatik hızlanma sistemi
            if (dir != 0 && IsGrounded())
            {
                // Yürüyorsan zamanla hızlan
                moveTime += Time.deltaTime;
                float t = Mathf.Clamp01(moveTime / accelerationTime);
                currentMoveSpeed = Mathf.Lerp(walkSpeed, maxSpeed, t);
            }
            else if (dir == 0)
            {
                // Duruyorsan yavaşça yavaşla
                moveTime = Mathf.Max(0, moveTime - Time.deltaTime * decelerationRate);
                float t = Mathf.Clamp01(moveTime / accelerationTime);
                currentMoveSpeed = Mathf.Lerp(walkSpeed, maxSpeed, t);
            }
            
            v.x = dir * currentMoveSpeed;
        }
        
        gravity.SetVelocity(v);
    }

    void OnDisable() => inputActions.Disable();
}