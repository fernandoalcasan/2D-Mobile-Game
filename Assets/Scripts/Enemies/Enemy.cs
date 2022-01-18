using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof (Rigidbody2D), typeof(AudioSource))]
public abstract class Enemy : MonoBehaviour, IDamageable
{
    public float Health { get; set; }
    [SerializeField]
    protected float health;
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float gems;
    [SerializeField]
    private GameObject _gemPrefab;
    [SerializeField]
    private GameObject _disappearPrefab;

    [SerializeField]
    private float attackDistance;
    [SerializeField]
    private float _knockbackForce;
    [SerializeField]
    private float _huntingSpeed;

    [SerializeField]
    private List<Transform> _waypoints;
    protected Animator _anim;
    protected Transform _player;
    protected Rigidbody2D _rb;
    protected Collider2D _collider;
    protected AudioSource _sfx;

    private Vector3 _currentTarget;
    private int _targetIndex;
    private int _idleAnimHash;
    protected int _walkAnimHash;
    protected int _hitAnimHash;
    protected int _attackAnimHash;
    protected int _deathAnimHash;
    private bool _onIdle;
    private Vector3 _facing;
    private bool _hunting;
    private bool _attacking;
    protected bool _gotHit;
    private bool _isDead;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _sfx = GetComponent<AudioSource>();

        if (_anim is null)
            Debug.LogError("The enemy animator is NULL!");

        if (_waypoints.Count < 2)
            Debug.LogError("Please select two or more waypoints for the enemy");

        _idleAnimHash = Animator.StringToHash("Idle");
        _hitAnimHash = Animator.StringToHash("GetHit");
        _attackAnimHash = Animator.StringToHash("Attack");
        _deathAnimHash = Animator.StringToHash("Death");
        _walkAnimHash = Animator.StringToHash("Walk");

        Health = health;
        _targetIndex = 1;
        _currentTarget = _waypoints[_targetIndex].position;
        RotateTowardsTarget(_currentTarget.x);
    }

    private void FixedUpdate()
    {
        if (!_hunting)
            MoveToWaypoint();
        else
            HuntPlayer();
    }

    private void MoveToWaypoint()
    {
        if (_onIdle)
            return;

        if(Vector2.SqrMagnitude(_currentTarget - transform.position) <= .05f)
        {
            //Idle when enemy arrives to a waypoint
            _onIdle = true;
            _anim.SetTrigger(_idleAnimHash);

            _targetIndex++;
            if(_targetIndex >= _waypoints.Count)
            {
                _waypoints.Reverse();
                _targetIndex = 1;
            }

            _currentTarget = _waypoints[_targetIndex].position;
        }

        _rb.MovePosition(Vector2.MoveTowards(transform.position, _currentTarget, speed * Time.fixedDeltaTime));
    }

    private void HuntPlayer()
    {
        if (_attacking || _gotHit)
            return;

        //Handle rotation towards the player
        if ((_facing.y == 0f && _player.position.x < transform.position.x) ||
            (_facing.y == 180f && _player.position.x > transform.position.x))
            RotateTowardsTarget(_player.position.x);

        if (Vector2.SqrMagnitude(_player.position - transform.position) <= attackDistance * attackDistance)
        {
            PerformAttack(_player.position);
            return;
        }

        Vector2 newVel2Move = _rb.velocity;
        newVel2Move.x = transform.right.normalized.x * _huntingSpeed;
        _rb.velocity = newVel2Move;
    }
    
    private void RotateTowardsTarget(float targetXPos)
    {
        _facing = transform.eulerAngles;
        if (targetXPos > transform.position.x)
            _facing.y = 0f;
        else
            _facing.y = 180f;

        transform.eulerAngles = _facing;
    }

    //Method called at the end of the idle animation
    private void StopIdle()
    {
        _onIdle = false;
        _anim.SetTrigger(_walkAnimHash);
        RotateTowardsTarget(_currentTarget.x);
    }

    public void IdentifyPlayer(Transform player)
    {
        if (_player is null)
            _player = player;

        _anim.SetTrigger(_walkAnimHash);
        _hunting = true;
        _onIdle = false;
    }

    public void ReturnToPatrol()
    {
        _hunting = false;
        _onIdle = true;
        _anim.SetTrigger(_idleAnimHash);
        _anim.ResetTrigger(_walkAnimHash);
    }

    //Method called after the attack state ends
    public void StopAttacking()
    {
        _attacking = false;
    }

    //Method called after the githit state ends
    public void StopHitFeedback()
    {
        _gotHit = false;
    }

    protected virtual void PerformAttack(Vector2 finalPos)
    {
        _anim.SetTrigger(_attackAnimHash);
        _attacking = true;
    }

    public virtual void Damage(Vector2 attackPos, float damage)
    {
        if (_isDead)
            return;

        _gotHit = true;
        Health -= damage;

        if (transform.position.x > attackPos.x)
            _rb.AddForce((Vector2.right + Vector2.up) * _knockbackForce, ForceMode2D.Impulse);
        else
            _rb.AddForce((Vector2.left + Vector2.up) * _knockbackForce, ForceMode2D.Impulse);

        if (Health <= 0f)
        {
            _isDead = true;
            for (int i = 0; i < gems; i++)
            {
                Instantiate(_gemPrefab, transform.position, Quaternion.identity);
            }
            _anim.SetTrigger(_deathAnimHash);
            StartCoroutine(DisplayDeathEffect());
            Destroy(gameObject, 2f);
        }
        else
            _anim.SetTrigger(_hitAnimHash);
    }

    private IEnumerator DisplayDeathEffect()
    {
        yield return new WaitForSeconds(0.5f);
        Vector2 pos = _collider.bounds.center;
        pos.y -= _collider.bounds.extents.y / 2;
        GameObject deathEffect = Instantiate(_disappearPrefab, pos, Quaternion.identity);
        Destroy(deathEffect, 2f);
    }

    protected void PlaySFX(SFX sfx)
    {
        _sfx.PlayOneShot(sfx.sound, sfx.volume);
    }
}
