using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamageable
{
    public float Health { get; set; }

    [SerializeField]
    private GameEvent _onPlayerDamaged;

    [Header("Player Properties")]
    [SerializeField]
    private PlayerData _playerData;

    [Header("Ground Check Properties")]
    [SerializeField]
    private Transform _groundCheckPoint;
    [SerializeField]
    private float _groundCheckHeight;
    [SerializeField]
    private float _slopeCheckHeight;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private float _disableGCTime;
    [SerializeField]
    private PhysicsMaterial2D _slippery;
    [SerializeField]
    private PhysicsMaterial2D _nonSlippery;
    [SerializeField]
    private SFX _landingSFX;

    [Header("Attack Properties")]
    [SerializeField]
    private Transform _attackPoint;
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private LayerMask _attackMask;
    [SerializeField]
    private float _attackCoolDown;

    //////Help vars////////
    //Movement
    private Vector2 _movement;
    private float _moveInput;
    private int _moveAnimHash;
    private Vector3 _facing;
    private bool _cantMove;

    //Jump & Ground check
    private bool _isGrounded;
    private Vector3 _feetPos;
    private Vector2 _boxSize;
    private Vector2 _slopePerp;
    private float _slopeAngle;
    private bool _isOnSlope;
    private bool _jumping;
    private float _initialGravityScale;
    private bool _groundCheckEnabled = true;
    private WaitForSeconds _wait;
    private int _jumpAnimHash;
    private int _doubleJumpHash;
    private bool _canDoubleJump;

    //Attack
    private int _attackAnimHash;
    private int _hitAnimHash;
    private int _deathAnimHash;
    private bool _canAttack = true;
    private WaitForSeconds _attackWait;

    //References
    private Rigidbody2D _rbody;
    private PlayerActions _playerActions;
    private Collider2D _collider;
    private Animator _animator;
    private PlayerEffects _effects;

    void Awake()
    {
        _playerActions = new PlayerActions();

        _rbody = GetComponent<Rigidbody2D>();
        if (_rbody is null)
            Debug.LogError("Rigidbody2D is NULL!");

        _collider = GetComponent<Collider2D>();
        if (_collider is null)
            Debug.Log("BoxCollider2D is NULL!");

        _animator = GetComponent<Animator>();
        if (_animator is null)
            Debug.Log("Animator is NULL!");

        _effects = GetComponentInChildren<PlayerEffects>();
        if (_effects is null)
            Debug.LogError("PlayerEffects in children is NULL!");

        Health = _playerData.maxHealth;
        _initialGravityScale = _rbody.gravityScale;
        _wait = new WaitForSeconds(_disableGCTime);
        _attackWait = new WaitForSeconds(_attackCoolDown);
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
        Diamond.OnDiamondCollected += CollectGem;
    }

    private void OnEnable()
    {
        _playerActions.Player_Map.Enable();
    }

    void FixedUpdate()
    {
        if (_groundCheckEnabled)
            GroundCheck();
        SlopeCheck();
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        if (_cantMove)
            return;

        if(_isGrounded && !_isOnSlope)
        {
            _movement.Set(_moveInput * _playerData.Speed, 0f);
            _rbody.velocity = _movement;
        }
        else if(_isGrounded && _isOnSlope)
        {
            _movement.Set(-_moveInput * _playerData.Speed * _slopePerp.x, -_moveInput * _playerData.Speed * _slopePerp.y);
            _rbody.velocity = _movement;
        }
        else if(!_isGrounded)
        {
            _movement.Set(_moveInput * _playerData.Speed, _rbody.velocity.y);
            _rbody.velocity = _movement;
        }
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        if (_cantMove && !context.canceled)
            return;

        if (context.canceled && _isOnSlope)
            _rbody.sharedMaterial = _nonSlippery;
        else
            _rbody.sharedMaterial = _slippery;

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
        if(_isGrounded)
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
        if(_canAttack && _isGrounded && Health > 0f)
        {
            StartCoroutine(StartAttackCoolDown());
            _animator.SetTrigger(_attackAnimHash);
            _effects.DisplayArc(_facing.y < 0f);
        }
    }

    private IEnumerator StartAttackCoolDown()
    {
        _canAttack = false;
        yield return _attackWait;
        _canAttack = true;
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
                hit.Damage(transform.position, _playerData.AttackPower);
            }
        }
    }

    /*private void OnDrawGizmos()
    {
        if (_attackPoint is null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackPoint.position, _attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _boxSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckHeight);
    }*/

    private void HandleGravity()
    {
        if (_jumping && _rbody.velocity.y < 0f) //Jump Fall
            _rbody.gravityScale = _initialGravityScale * _playerData.jumpFallGravityMultiplier;
    }

    private IEnumerator EnableGroundCheckAfterJump()
    {
        _groundCheckEnabled = false;
        _isGrounded = false;
        yield return _wait;
        _groundCheckEnabled = true;
    }

    private void SlopeCheck()
    {
        //Slope check from feet origin
        _feetPos = _collider.bounds.center;
        _feetPos.y -= _collider.bounds.extents.y;
        var groundCastHit = Physics2D.Raycast(_feetPos, Vector2.down, _slopeCheckHeight, _groundMask);

        if (groundCastHit)
        {
            _slopePerp = Vector2.Perpendicular(groundCastHit.normal).normalized;
            _slopeAngle = Vector2.Angle(groundCastHit.normal, Vector2.up);

            if (_slopeAngle != 0f)
                _isOnSlope = true;
            else
                _isOnSlope = false;
        }
    }

    private void GroundCheck()
    {
        var groundCast = Physics2D.OverlapBox(_groundCheckPoint.position, _boxSize, 0f, _groundMask);

        if(!groundCast)
        {
            _isGrounded = false;
        }
        //When player is landing from a jump
        else if (_jumping && groundCast)
        {
            if(_rbody.velocity.y < 1f || _isOnSlope)
            {
                _isGrounded = true;
                _animator.SetBool(_jumpAnimHash, false);
                _jumping = false;
                _canDoubleJump = false;
                _rbody.gravityScale = _initialGravityScale;
                AudioManager.Instance.PlayOneShotSFX(_landingSFX.sound, _landingSFX.volume);
            }
        }
        //When player is not jumping and is on the ground
        else if (!_jumping && groundCast)
            _isGrounded = true;

        //If player is jumping and going up _isGrounded remains without change
    }

    private void CollectGem()
    {
        _playerData.diamonds++;
    }

    private void OnDisable()
    {
        _playerActions.Player_Map.Disable();
        Diamond.OnDiamondCollected -= CollectGem;
    }

    public void Damage(Vector2 attackPos, float damage)
    {
        if (Health <= 0f)
            return;

        Health -= damage;
        _playerData.health = Health < 0f ? 0f : Health;
        _onPlayerDamaged.Raise();

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

    public void UpgradeAttackPower()
    {
        _playerData.gotAttackUpgrade = true;
        _animator.runtimeAnimatorController = _playerData.fireOverride;
    }

    public void UpgradeSpeed()
    {
        _playerData.gotWindBoots = true;
    }

    public void GetCastleKey()
    {
        _playerData.gotCastleKey = true;
    }

    private void SavePlayer()
    {
        SaveManager.SavePlayerData(_playerData);
    }

    private void LoadPlayer()
    {
        PlayerData load = SaveManager.LoadPlayerData();
        if (!(load is null))
            _playerData = load;
    }

    public void HandleControls(bool enable)
    {
        if(enable)
            _playerActions.Player_Map.Enable();
        else
            _playerActions.Player_Map.Disable();
    }

    private void PlaySFX(SFX sfx)
    {
        AudioManager.Instance.PlayOneShotSFX(sfx.sound, sfx.volume);
    }

    private void PlayFootstep(SFX footstep)
    {
        if(_isGrounded)
            AudioManager.Instance.PlayOneShotSFX(footstep.sound, footstep.volume);
    }
}
