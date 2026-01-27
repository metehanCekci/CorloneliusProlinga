using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerScript : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 5f; // Maksimum hız
    [SerializeField] private float acceleration = 8f; // İvme (hızlanma/yavaşlama)
    
    private Gravity gravity;
    private float currentSpeed = 0f;
    private float moveDirection = 0f;
    private float previousDirection = 0f;
    
    private InputActions inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;

    void OnEnable()
    {
        // Input Actions'u al
        inputActions = new InputActions();
        inputActions.Enable();
        moveAction = inputActions.Player.Move;
        jumpAction = inputActions.Player.Jump;
        
        jumpAction.performed += OnJumpStart;
        jumpAction.canceled += OnJumpEnd;
    }

    void OnDisable()
    {
        jumpAction.performed -= OnJumpStart;
        jumpAction.canceled -= OnJumpEnd;
        inputActions.Disable();
        inputActions.Dispose();
    }

    void Start()
    {
        gravity = GetComponent<Gravity>();
    }

    void Update()
    {
        HandleInput();
        ApplyMovement();
    }

    private void HandleInput()
    {
        // Yatay input
        moveDirection = moveAction.ReadValue<float>();
    }

    private void OnJumpStart(InputAction.CallbackContext context)
    {
        gravity.StartJump();
    }

    private void OnJumpEnd(InputAction.CallbackContext context)
    {
        gravity.EndJump();
    }

    private void ApplyMovement()
    {
        // Tuş basılıysa hızlanma, bırakıldıysa yavaşlama
        if (moveDirection != 0)
        {
            // Tuş basılı - ivme ile hızlanır
            currentSpeed += acceleration * moveDirection * Time.deltaTime;
            
            // Max hız limiti
            if (Mathf.Abs(currentSpeed) > maxSpeed)
            {
                currentSpeed = Mathf.Sign(currentSpeed) * maxSpeed;
            }
        }
        else
        {
            // Tuş bırakıldı - ivme ile yavaşlar
            if (currentSpeed > 0)
            {
                currentSpeed -= acceleration * Time.deltaTime;
                if (currentSpeed < 0) currentSpeed = 0;
            }
            else if (currentSpeed < 0)
            {
                currentSpeed += acceleration * Time.deltaTime;
                if (currentSpeed > 0) currentSpeed = 0;
            }
        }

        // Hareket uygulanıyor
        Vector3 velocity = gravity.GetVelocity();
        velocity.x = currentSpeed;
        
        // Gravity scriptine yeni velocity'yi ayarla
        UpdateGravityVelocity(velocity);
    }

    // Gravity scriptinin velocity'sini güncelle
    private void UpdateGravityVelocity(Vector3 newVelocity)
    {
        gravity.SetVelocity(newVelocity);
    }
}
