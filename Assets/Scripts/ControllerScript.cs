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
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashDirection = 0f;

    [Header("Görsel Efektler (Juice & Dust)")]
    [SerializeField] private GameObject DustEffectPrefab;
    private JuiceEffect juice;

    private Gravity gravity;
    private InputActions inputActions;
    private InputAction moveAction, jumpAction, pushAction, brakeAction, dashAction;

    void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        pushAction = inputActions.Player.Push;
        brakeAction = inputActions.Player.Brake;
        dashAction = inputActions.Player.Dash;

        brakeAction.performed += OnBrake;
        jumpAction.performed += OnJumpStart;
        jumpAction.canceled += OnJumpCancel;
        dashAction.performed += OnDash;
    }

    void Start()
    {
        gravity = GetComponent<Gravity>();
        juice = GetComponentInChildren<JuiceEffect>(); // Child'daki scripti bul
    }

    void Update()
    {
        if (railCooldown > 0) railCooldown -= Time.deltaTime;
        if (momentumTime > 0) momentumTime -= Time.deltaTime;

        // Yer Kontrolü ve Yere İnme (Squish) Tetikleyici
        bool grounded = IsGrounded();
        if (grounded && !wasGrounded)
        {
            HasDash = true; 
            HasSecondJump = true;
            juice?.ApplySquish(); // Yere iniş efekti
        }
        wasGrounded = grounded;

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
            }
        }

        if (isGrinding) return;
        ApplyMovement();
    }

    public bool IsGrounded() => gravity != null && gravity.IsGrounded();

    private void OnJumpStart(InputAction.CallbackContext context)
    {
        if (isGrinding)
        {
            if (activeRail != null) activeRail.FinishGrind(false);
            gravity.StartJump(); 
            juice?.ApplyStretch(); // Zıplama efekti
            SpawnDust();
            return;
        }

        if (IsGrounded() || HasSecondJump)
        {
            if (!IsGrounded()) HasSecondJump = false;
            gravity.StartJump();
            juice?.ApplyStretch(); // Zıplama efekti
            SpawnDust();
        }
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
        if (!HasDash || isGrinding || isDashing) return;

        HasDash = false;
        isDashing = true;
        dashTimer = dashDuration;
        storedMoveSpeed = currentMoveSpeed;

        float input = moveAction.ReadValue<float>();
        dashDirection = input != 0 ? Mathf.Sign(input) : (transform.localScale.x >= 0 ? 1f : -1f);
        
        juice?.ApplyDashStretch(); // Dash efekti
    }

    public void ResetSpeed() { currentMoveSpeed = walkSpeed; }
    public bool CanEnterRail() => railCooldown <= 0 && !isGrinding;

    public void EnterRail(float entrySpeed, RailSystem rail)
    {
        isGrinding = true;
        activeRail = rail;
        HasDash = true;
        HasSecondJump = true;
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