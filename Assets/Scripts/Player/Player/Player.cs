/*
 * This script contains the player's behavior and mechanics.
 * It implements the IDamageable interface (health system) and
 * it controls the player through a rigidbody component
 */

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamageable
{
    public float Health { get; set; }

    [Header("Player Events")]
    [SerializeField]
    private GameEvent OnPlayerDamaged;
    [SerializeField]
    private GameEvent OnPlayerDeath;

    [Header("Player Properties")]
    [SerializeField]
    private PlayerData _playerData;
    [SerializeField]
    private AnimatorOverrideController _fireOverride;

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

        LoadPlayer();

        Health = _playerData.data.maxHealth;
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

    private void Start()
    {
        CheckForUpgrades();
    }

    //Enable the player controls
    private void OnEnable()
    {
        _playerActions.Player_Map.Enable();
    }

    //Handle player's behavior and physics
    void FixedUpdate()
    {
        if (_groundCheckEnabled)
            GroundCheck();
        SlopeCheck();
        HandleMovement();
        HandleGravity();
    }

    //Handles player movement depending on ground slopes
    private void HandleMovement()
    {
        if (!_playerActions.Player_Map.enabled)
            return;

        if(_isGrounded && !_isOnSlope)
        {
            _movement.Set(_moveInput * _playerData.data.Speed, 0f);
            _rbody.velocity = _movement;
        }
        else if(_isGrounded && _isOnSlope)
        {
            _movement.Set(-_moveInput * _playerData.data.Speed * _slopePerp.x, -_moveInput * _playerData.data.Speed * _slopePerp.y);
            _rbody.velocity = _movement;
        }
        else if(!_isGrounded)
        {
            _movement.Set(_moveInput * _playerData.data.Speed, _rbody.velocity.y);
            _rbody.velocity = _movement;
        }
    }

    //Handles movement action input (new input system)
    private void OnMovementInput(InputAction.CallbackContext context)
    {
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

    //Handles jump action input (new input system)
    private void Jump_performed(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _rbody.velocity = Vector2.up * _playerData.data.jumpPower;
            _animator.SetBool(_jumpAnimHash, true);
            _jumping = true;
            _canDoubleJump = true;
            StartCoroutine(EnableGroundCheckAfterJump());
        }
        else if(_canDoubleJump)
        {
            _canDoubleJump = false;
            _rbody.velocity = Vector2.up * _playerData.data.jumpPower;
            _animator.SetTrigger(_doubleJumpHash);
        }
    }

    //Handles attack action input (new input system)
    private void Attack_performed(InputAction.CallbackContext context)
    {
        if(_canAttack && _isGrounded && Health > 0f)
        {
            StartCoroutine(StartAttackCoolDown());
            _animator.SetTrigger(_attackAnimHash);
            _effects.DisplayArc(_facing.y < 0f);
        }
    }

    //Handles attack cooldown to avoid spamming attacks
    private IEnumerator StartAttackCoolDown()
    {
        _canAttack = false;
        yield return _attackWait;
        _canAttack = true;
    }

    //Method called from an attack animation event to perform damage to IDamageable components in attack range
    private void Attack()
    {
        _rbody.velocity = Vector2.zero;
        Collider2D[] objs = Physics2D.OverlapCircleAll(_attackPoint.position, _attackRange, _attackMask);

        foreach (var obj in objs)
        {
            if(obj.TryGetComponent(out IDamageable hit))
            {
                hit.Damage(transform.position, _playerData.data.AttackPower);
            }
        }
    }

    //Remove to draw the attack range or the ground check range while in Editor mode
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

    //Changes gravity when falling from a jump to perform a better jump
    private void HandleGravity()
    {
        if (_jumping && _rbody.velocity.y < 0f) //Jump Fall
            _rbody.gravityScale = _initialGravityScale * _playerData.data.jumpFallGravityMultiplier;
    }

    //Coroutine to disable and enable again the ground check after every jump (to avoid issues with one-way platforms)
    private IEnumerator EnableGroundCheckAfterJump()
    {
        _groundCheckEnabled = false;
        _isGrounded = false;
        yield return _wait;
        _groundCheckEnabled = true;
    }

    //Method to check if the player is in a slope or not
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

    //Method to check if the player is grounded
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

    //Method subscribed to the diamond collection event in order to update diamonds data for the player
    private void CollectGem()
    {
        _playerData.data.diamonds++;
    }

    private void OnDisable()
    {
        _playerActions.Player_Map.Disable();
        Diamond.OnDiamondCollected -= CollectGem;
    }

    //Method implemented by the IDamageable interface to handle the health and damage system
    public void Damage(Vector2 attackPos, float damage)
    {
        if (Health <= 0f)
            return;

        Health -= damage;
        _playerData.data.health = Health < 0f ? 0f : Health;
        OnPlayerDamaged.Raise();

        if (transform.position.x > attackPos.x)
            _rbody.AddForce((Vector2.right + Vector2.up) * _playerData.data.knockbackForce, ForceMode2D.Impulse);
        else
            _rbody.AddForce((Vector2.left + Vector2.up) * _playerData.data.knockbackForce, ForceMode2D.Impulse);

        if (Health <= 0f)
        {
            SavePlayerData();
            _animator.SetTrigger(_deathAnimHash);
            OnPlayerDeath.Raise();
            HandleControls(false);
            _rbody.sharedMaterial = _nonSlippery;
        }
        else
            _animator.SetTrigger(_hitAnimHash);
    }

    //Method called when certain item is acquired
    public void UpgradeAttackPower()
    {
        _playerData.data.gotAttackUpgrade = true;
        _animator.runtimeAnimatorController = _fireOverride;
    }

    //Method called when certain item is acquired
    public void UpgradeSpeed()
    {
        _playerData.data.gotWindBoots = true;
    }

    //Method called when certain item is acquired
    public void GetCastleKey()
    {
        _playerData.data.gotCastleKey = true;
    }

    //Method called to save the player's data locally
    public void SavePlayerData()
    {
        SaveManager.SavePlayerData(_playerData.data);
    }

    //Method called to load the player's local data
    private void LoadPlayer()
    {
        Data load = SaveManager.LoadPlayerData();
        if (!(load is null))
        {
            _playerData.data = load;
        }
    }

    //Method called at start to make sure that upgrades are implemented
    private void CheckForUpgrades()
    {
        if (_playerData.data.gotAttackUpgrade)
        {
            _animator.runtimeAnimatorController = _fireOverride;
            _effects.UpgradeEffects();
        }
    }

    //Method to enable or disable the player controls
    public void HandleControls(bool enable)
    {
        if(enable)
        {
            _playerActions.Player_Map.Enable();
            _rbody.sharedMaterial = _slippery;
        }
        else
        {
            _playerActions.Player_Map.Disable();
            _rbody.sharedMaterial = _nonSlippery;
        }
    }

    //Method called by animation events to play SFX with scriptable objects
    private void PlaySFX(SFX sfx)
    {
        AudioManager.Instance.PlayOneShotSFX(sfx.sound, sfx.volume);
    }

    //Method called by the walking animation to play the footstep SFX
    //with scriptable objects when the player is grounded
    private void PlayFootstep(SFX footstep)
    {
        if(_isGrounded)
            AudioManager.Instance.PlayOneShotSFX(footstep.sound, footstep.volume);
    }
}
