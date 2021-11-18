using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamageable
{
    public float Health { get; set; }

    [Header("Player Properties")]
    [SerializeField]
    private PlayerData _playerData;

    [Header("Ground Check Properties")]
    [SerializeField]
    private float _groundCheckHeight;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private float _disableGCTime;

    [Header("Attack Properties")]
    [SerializeField]
    private Transform _attackPoint;
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private LayerMask _attackMask;

    //////Help vars////////
    //Movement
    private float _moveInput;
    private int _moveAnimHash;
    private Vector3 _facing;
    private bool _cantMove;

    //Jump & Ground check
    private Vector3 _boxCenter;
    private Vector2 _boxSize;
    private bool _jumping;
    private float _initialGravityScale;
    private bool _groundCheckEnabled = true;
    private WaitForSeconds _wait;
    private int _jumpAnimHash;
    private int _doubleJumpHash;
    private bool _canDoubleJump;
    private bool _keepGrounded;

    //Attack
    private int _attackAnimHash;
    private int _hitAnimHash;
    private int _deathAnimHash;

    //References
    private Rigidbody2D _rbody;
    private PlayerActions _playerActions;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerEffects _effects;

    void Awake()
    {
        _playerActions = new PlayerActions();

        _rbody = GetComponent<Rigidbody2D>();
        if (_rbody is null)
            Debug.LogError("Rigidbody2D is NULL!");

        _collider = GetComponent<BoxCollider2D>();
        if (_collider is null)
            Debug.Log("BoxCollider2D is NULL!");

        _animator = GetComponent<Animator>();
        if (_animator is null)
            Debug.Log("Animator is NULL!");

        _effects = GetComponentInChildren<PlayerEffects>();
        if (_effects is null)
            Debug.LogError("PlayerEffects in children is NULL!");

        Health = _playerData.health;
        _initialGravityScale = _rbody.gravityScale;
        _wait = new WaitForSeconds(_disableGCTime);
        _moveAnimHash = Animator.StringToHash("Movement");
        _jumpAnimHash = Animator.StringToHash("Jumping");
        _attackAnimHash = Animator.StringToHash("Attack");
        _doubleJumpHash = Animator.StringToHash("DoubleJump");
        _hitAnimHash = Animator.StringToHash("GetHit");
        _deathAnimHash = Animator.StringToHash("Death");

        //Size of the ground checking box (width, height)
        _boxSize = new Vector2(_collider.bounds.size.x - 0.1f, _groundCheckHeight);

        //Methods subscribed to actions from input system
        _playerActions.Player_Map.Movement.started += OnMovementInput;
        _playerActions.Player_Map.Movement.performed += OnMovementInput;
        _playerActions.Player_Map.Movement.canceled += OnMovementInput;
        _playerActions.Player_Map.Jump.performed += Jump_performed;
        _playerActions.Player_Map.Attack.performed += Attack_performed;
        Diamond.OnDiamondCollected += () => _playerData.diamonds++;
    }

    private void OnEnable()
    {
        _playerActions.Player_Map.Enable();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        if (_cantMove)
            return;

        Vector2 movement = _rbody.velocity;
        movement.x = _moveInput * _playerData.speed;
        _rbody.velocity = movement;
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        if (_cantMove && !context.canceled)
            return;

        //Get input value
        _moveInput = context.ReadValue<float>();

        //Flip character by rotation on Y axis depending on movement
        if(_moveInput != 0f)
        {
            _facing = transform.eulerAngles;
            _facing.y = _moveInput > 0 ? 0f : 180f;
            transform.eulerAngles = _facing;
        }

        //Set animation float value
        _animator.SetFloat(_moveAnimHash, Mathf.Abs(_moveInput));
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        if(IsGrounded())
        {
            _rbody.velocity = Vector2.up * _playerData.jumpPower;
            _animator.SetBool(_jumpAnimHash, true);
            _jumping = true;
            _canDoubleJump = true;
            StartCoroutine(EnableGroundCheckAfterJump());
        }
        else if(_canDoubleJump)
        {
            _canDoubleJump = false;
            _rbody.velocity = Vector2.up * _playerData.jumpPower;
            _animator.SetTrigger(_doubleJumpHash);
        }
    }
    
    private void Attack_performed(InputAction.CallbackContext context)
    {
        if(IsGrounded())
        {
            _animator.SetTrigger(_attackAnimHash);
            _effects.DisplayArc(_facing.y < 0f);
        }
    }

    //Method called from attack animation
    private void Attack()
    {
        _rbody.velocity = Vector2.zero;
        Collider2D[] objs = Physics2D.OverlapCircleAll(_attackPoint.position, _attackRange, _attackMask);

        foreach (var obj in objs)
        {
            if(obj.TryGetComponent(out IDamageable hit))
            {
                hit.Damage(transform.position, _playerData.attackPower);
            }
        }
    }

    /*private void OnDrawGizmos()
    {
        if (_attackPoint is null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackPoint.position, _attackRange);
    }*/

    private void HandleGravity()
    {
        if (_groundCheckEnabled)
            IsGrounded();

        if (_jumping && _rbody.velocity.y < 0f) //Jump Fall
            _rbody.gravityScale = _initialGravityScale * _playerData.jumpFallGravityMultiplier;
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
        {
            if(!_keepGrounded)
            {
                _keepGrounded = true;
                _animator.SetBool(_jumpAnimHash, false);
                _jumping = false;
                _canDoubleJump = false;
                _rbody.gravityScale = _initialGravityScale;
            }
            return true;
        }
        _keepGrounded = false;
        return false;
    }

    private void OnDisable()
    {
        _playerActions.Player_Map.Disable();
    }

    public void Damage(Vector2 attackPos, float damage)
    {
        Health -= damage;

        if (transform.position.x > attackPos.x)
            _rbody.AddForce((Vector2.right + Vector2.up) * _playerData.knockbackForce, ForceMode2D.Impulse);
        else
            _rbody.AddForce((Vector2.left + Vector2.up) * _playerData.knockbackForce, ForceMode2D.Impulse);

        if (Health <= 0f)
        {
            _animator.SetTrigger(_deathAnimHash);
            //dead is true
            //Destroy(gameObject, 2f);
        }
        else
            _animator.SetTrigger(_hitAnimHash);
    }

    public void EnableMovement()
    {
        _cantMove = false;
    }

    public void DisableMovement()
    {
        _cantMove = true;
    }

    public bool SpendDiamonds(int value)
    {
        if(value <= _playerData.diamonds)
        {
            _playerData.diamonds -= value;
            return true;
        }

        return false;
    }

    public void UpgradeAttackPower(float percentage)
    {
        _playerData.attackPower *= (100f + percentage) / 100;
    }

    public void UpgradeSpeed(float percentage)
    {
        _playerData.speed *= (100f + percentage) / 100;
    }

    public void GetCastleKey()
    {
        _playerData.gotCastleKey = true;
    }
}
