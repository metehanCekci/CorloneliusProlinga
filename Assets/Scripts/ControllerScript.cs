using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ControllerScript : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer; // Müfettişten (Inspector) "Ground" katmanını seç!
    [SerializeField] private float maxSpeed = 5f; // Maksimum hız
    [SerializeField] private float acceleration = 8f; // İvme (hızlanma/yavaşlama)
    [SerializeField] private float dashDecay = 10f;
    private float currentDashSpeed = 0f; // O anki dash kuvveti

    [SerializeField] public int DashPower;

    private Gravity gravity;
    private float currentSpeed = 0f;
    private float moveDirection = 0f;
    private float previousDirection = 0f;

    private InputActions inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction DashAction;
    public bool HasDash;
    public bool HasSecondJump;

    void OnEnable()
    {
        // Input Actions'u al
        inputActions = new InputActions();
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        DashAction = inputActions.Player.Dash;

        jumpAction.performed += OnJumpStart;
        jumpAction.canceled += OnJumpEnd;
        DashAction.performed += OnDashStart;
    }

    void OnDisable()
    {
        jumpAction.performed -= OnJumpStart;
        jumpAction.canceled -= OnJumpEnd;
        DashAction.performed -= OnDashStart;
        inputActions.Disable();
        inputActions.Dispose();
    }

    void Start()
    {
        gravity = GetComponent<Gravity>();
    }

    void Update()
    {
        moveDirection = moveAction.ReadValue<float>();
        // --- 1. YER KONTROLÜ VE RESET ---
        if (IsGrounded())
        {
            HasDash = true;
        }
        ApplyMovement();
    }
    private void OnDashStart(InputAction.CallbackContext context)
    {
        // Eğer dash hakkımız varsa (yere değince HasDash true olmuştu)
        if (moveDirection != 0 && HasDash)
        {
            currentDashSpeed = moveDirection * DashPower;
            HasDash = false; // Hakkı bitir
        }
    }

    private void OnJumpStart(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            gravity.StartJump();
        }
        else if (HasSecondJump)
        {
            Vector3 tempVel = gravity.GetVelocity();
        tempVel.y = 0;
        gravity.SetVelocity(tempVel);

        gravity.StartJump();
        HasSecondJump = false;
        }   
    }

    private void OnJumpEnd(InputAction.CallbackContext context)
    {
        gravity.EndJump();
    }

    private bool IsGrounded()
    {
        // Karakterin merkezinden aşağıya doğru ışın fırlatıyoruz
        // 1.1f değeri karakterin boyuna göre ayarlanmalı (yarı boyundan biraz fazla)
        float rayDistance = 1.2f;
        // Sadece "Ground" katmanındaki objeleri algılaması için LayerMask kullanmak en iyisidir
        // Şimdilik basitçe herhangi bir şeye çarpıp çarpmadığına bakalım:
        return Physics2D.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);
    }

    private void ApplyMovement()
    {
        if (IsGrounded())
        {
            HasDash = true; // Yere değdiğimiz anda dash hakkımız geri gelir
            HasSecondJump = true;
        }
        // Tuş basılıysa hızlanma, bırakıldıysa yavaşlama
        if (moveDirection != 0)
        {
            // Tuş basılı - ivme ile hızlanır
            currentSpeed += acceleration * moveDirection * Time.deltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, acceleration * Time.deltaTime);
        }

        // Yürüme hızını KENDİ İÇİNDE sınırla (Dash'i etkilemez)
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // --- 2. DASH SÖNÜMLEME MANTIĞI ---
        // Dash hızını her karede sıfıra yaklaştırıyoruz
        currentDashSpeed = Mathf.MoveTowards(currentDashSpeed, 0, dashDecay * Time.deltaTime);

        // --- 3. BİRLEŞTİRME ---
        Vector3 velocity = gravity.GetVelocity();

        // Yürüme hızı ve Dash hızını topluyoruz!
        // Böylece yürüme hızı 5 olsa bile dash 10 ise toplam 15 olur.
        velocity.x = currentSpeed + currentDashSpeed;

        UpdateGravityVelocity(velocity);
    }

    // Gravity scriptinin velocity'sini güncelle
    private void UpdateGravityVelocity(Vector3 newVelocity)
    {
        gravity.SetVelocity(newVelocity);
    }
}
