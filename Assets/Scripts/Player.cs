using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Player Properties")]
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _jumpPower;
    [SerializeField] [Range(1f, 5f)]
    private float _jumpFallGravityMultiplier;

    [Header("Ground Check Properties")]
    [SerializeField]
    private float _groundCheckHeight;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private float _disableGCTime;

    //////Help vars////////
    //Movement
    private float _moveInput;
    private int _moveAnimHash;

    //Jump & Ground check
    private Vector3 _boxCenter;
    private Vector2 _boxSize;
    private bool _jumping;
    private float _initialGravityScale;
    private bool _groundCheckEnabled = true;
    private WaitForSeconds _wait;

    //References
    private Rigidbody2D _rbody;
    private PlayerActions _playerActions;
    private BoxCollider2D _collider;
    private Animator _animator;

    void Awake()
    {
        _playerActions = new PlayerActions();

        _rbody = GetComponent<Rigidbody2D>();
        if (_rbody is null)
            Debug.LogError("Rigidbody2D is NULL!");

        _initialGravityScale = _rbody.gravityScale;

        _collider = GetComponent<BoxCollider2D>();
        if (_collider is null)
            Debug.Log("BoxCollider2D is NULL!");

        _wait = new WaitForSeconds(_disableGCTime);

        //Size of the ground checking box (width, height)
        _boxSize = new Vector2(_collider.bounds.size.x, _groundCheckHeight);

        _animator = GetComponent<Animator>();
        if (_animator is null)
            Debug.Log("Animator is NULL!");

        _moveAnimHash = Animator.StringToHash("Movement");

        _playerActions.Player_Map.Jump.performed += Jump_performed;
    }

    private void OnEnable()
    {
        _playerActions.Player_Map.Enable();
    }

    void FixedUpdate()
    {
        HandleMovement();
        Vector2 movement = _rbody.velocity;
        movement.x = _moveInput;
        _rbody.velocity = movement;

        HandleGravity();
    }

    private void HandleMovement()
    {
        //Get input value
        _moveInput = _playerActions.Player_Map.Movement.ReadValue<float>() * _speed;

        //Set animation float value
        _animator.SetFloat(_moveAnimHash, Mathf.Abs(_moveInput));
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        if(IsGrounded())
        {
            _rbody.velocity += Vector2.up * _jumpPower;
            _jumping = true;
            StartCoroutine(EnableGroundCheckAfterJump());
        }
    }

    private void HandleGravity()
    {
        if(_groundCheckEnabled && IsGrounded())
        {
            _jumping = false;
        }
        else if (_jumping && _rbody.velocity.y < 0f) //Jump Fall
        {
            _rbody.gravityScale = _initialGravityScale * _jumpFallGravityMultiplier;
        }
        else //Normal Fall
        {
            _rbody.gravityScale = _initialGravityScale;
        }
    }

    private IEnumerator EnableGroundCheckAfterJump()
    {
        _groundCheckEnabled = false;
        yield return _wait;
        _groundCheckEnabled = true;
    }

    private bool IsGrounded()
    {
        //Center coordinate of box that checks if player is touching the ground
        _boxCenter = _collider.bounds.center;
        _boxCenter.y -= _collider.bounds.extents.y + (_groundCheckHeight / 2f);

        var groundBox = Physics2D.OverlapBox(_boxCenter, _boxSize, 0f, _groundMask);

        if(groundBox != null)
            return true;
        return false;
    }

    private void OnDisable()
    {
        _playerActions.Player_Map.Disable();
    }
}