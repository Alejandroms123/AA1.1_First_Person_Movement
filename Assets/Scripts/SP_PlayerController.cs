using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SP_PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayer;

    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _strafeSpeed;
    [SerializeField] private float _backwardSpeed;

    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpbufferTime;

    [SerializeField] private float _jetpackPower;
    [SerializeField] private float _jetpackDuration;
    [SerializeField] private float _jetpackRegenTime;
    [SerializeField] private float _jetpackCooldown;

    private InputAction _moveInput;
    private InputAction _sprintInput;
    private InputAction _jumpInput;
    private InputAction _jetpack;

    private Camera _cam;
    private Collider _col;
    private Rigidbody _rb;
    private Animator _anim;
    private Slider _slider;
    private TMP_Text _txt;

    private float _speed;
    private float _jumpBufferTimer;
    private float _jetpackTimer;
    private float _jetpackCooldownTimer;
    private bool _isGrounded;
    private bool _jetpackInput;
    private bool _useJetpack;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _cam = Camera.main;
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _slider = FindAnyObjectByType<Slider>();
        _txt = FindAnyObjectByType<TMP_Text>();

        var playerInput = GetComponent<PlayerInput>();
        _moveInput = playerInput.actions["Move"];
        _sprintInput = playerInput.actions["Sprint"];
        _jumpInput = playerInput.actions["Jump"];
        _jetpack = playerInput.actions["Jetpack"];

        _jumpInput.performed += OnJumpPressed;
        _jetpack.performed += context => { _jetpackInput = true; };
        _jetpack.canceled += context => { _jetpackInput = false; };

        _speed = _walkSpeed;
        _jetpackTimer = _jetpackDuration;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, _cam.transform.eulerAngles.y, 0f);

        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= Time.deltaTime;

        _isGrounded = IsGrounded();

        if (!_isGrounded && _jetpackInput && _jetpackTimer > 0f)
            _useJetpack = true;
        else
            _useJetpack = false;

        if (_isGrounded)
            RegenJetpack();

        UpdateAnimator();

        _slider.value = _jetpackTimer / _jetpackDuration;
    }

    private void FixedUpdate()
    {
        if (_isGrounded && _jumpBufferTimer > 0f)
            Jump();
        else if (_useJetpack)
            UseJetpack();

        Vector2 moveInput = _moveInput.ReadValue<Vector2>();
        Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        moveDir.y = 0f;
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);

        ChangeSpeed(moveInput);
        Move(moveDir);
        _txt.text = $"FlatVel: {new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).magnitude:F2}";
    }

    private bool IsGrounded()
    {
        Vector3 origin = _col.bounds.center;
        float distance = _col.bounds.extents.y + 0.1f;

        return Physics.Raycast(origin, Vector3.down, distance, _groundLayer);
    }

    private void ChangeSpeed(Vector2 moveInput)
    {
        if (_sprintInput.IsPressed() && moveInput.y > 0f)
        {
            _speed = _sprintSpeed;
        }
        else if (moveInput.x != 0f && moveInput.y == 0f)
        {
            _speed = _strafeSpeed;
        }
        else if (moveInput.y < 0f)
        {
            _speed = _backwardSpeed;
        }
        else
        {
            _speed = _walkSpeed;
        }

        if (!_isGrounded)
            _speed *= 0.5f;
    }

    private void Move(Vector3 moveDir)
    {
        Vector3 targetVel = moveDir * _speed;
        Vector3 currentVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        Vector3 velocityDif = targetVel - currentVel;
        _rb.AddForce(velocityDif, ForceMode.VelocityChange);
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        _jumpBufferTimer = _jumpbufferTime;
    }

    private void Jump()
    {
        Vector3 vel = _rb.linearVelocity;
        vel.y = 0f;
        _rb.linearVelocity = vel;

        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        _jumpBufferTimer = 0f;
    }

    private void UseJetpack()
    {
        _jetpackTimer -= Time.fixedDeltaTime;
        _jetpackTimer = Mathf.Max(_jetpackTimer, 0f);
        _rb.AddForce(Vector3.up * _jetpackPower, ForceMode.Acceleration);
    }

    private void RegenJetpack()
    {
        if (_jetpackTimer <= 0f)
        {
            if (_jetpackCooldownTimer <= 0f)
                _jetpackCooldownTimer = _jetpackCooldown;

            _jetpackCooldownTimer -= Time.deltaTime;

            if (_jetpackCooldownTimer > 0f)
                return;
        }

        float regenRate = _jetpackDuration / _jetpackRegenTime;
        _jetpackTimer += regenRate * Time.deltaTime;
        _jetpackTimer = Mathf.Min(_jetpackTimer, _jetpackDuration);
    }

    private void UpdateAnimator()
    {
        _anim.SetBool("IsGrounded", _isGrounded);

        Vector3 localVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
        float velocityX = Mathf.Round(localVelocity.x);
        float velocityZ = Mathf.Round(localVelocity.z);
        velocityX = Mathf.Clamp(velocityX, -1f, 1f);
        velocityZ = Mathf.Clamp(velocityZ, -1f, 1f);

        _anim.SetFloat("VelocityX", velocityX);
        _anim.SetFloat("VelocityZ", velocityZ);
    }
}