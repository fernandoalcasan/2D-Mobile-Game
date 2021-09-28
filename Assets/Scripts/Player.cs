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

    //Help vars
    private float _moveInput;
    private Vector2 _boxCenter;
    private Vector2 _boxSize;
    private bool _jumping;
    private float _initialGravityScale;
    private bool _groundCheckEnabled = true;
    private WaitForSeconds _wait;

    //References
    private Rigidbody2D _rbody;
    private PlayerActions _playerActions;
    private BoxCollider2D _collider;

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

        _playerActions.Player_Map.Jump.performed += Jump_performed;
    }

    private void OnEnable()
    {
        _playerActions.Player_Map.Enable();
    }

    void FixedUpdate()
    {
        HandleMovement();
        _rbody.velocity = new Vector2(_moveInput, _rbody.velocity.y);

        HandleGravity();
    }

    private void HandleMovement()
    {
        _moveInput = _playerActions.Player_Map.Movement.ReadValue<float>() * _speed;
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

    private void OnDrawGizmos()
    {
        if(_jumping)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_boxCenter, _boxSize);
    }

    private bool IsGrounded()
    {
        //Center coordinate of box that checks if player is touching the ground
        _boxCenter = new Vector2(_collider.bounds.center.x, _collider.bounds.center.y) +
                     (Vector2.down * (_collider.bounds.extents.y + (_groundCheckHeight / 2f)));

        //Size of the box (width, height)
        _boxSize = new Vector2(_collider.bounds.size.x, _groundCheckHeight);

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
