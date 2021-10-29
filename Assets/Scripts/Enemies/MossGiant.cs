using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MossGiant : Enemy, IDamageable
{
    public int Health { get; set; }

    [SerializeField]
    private float _knockbackForce;

    private void Start()
    {
        Health = health;
    }

    public void Damage()
    {
        _gotHit = true;
        Health--;

        if (transform.position.x > _player.position.x)
            _rb.AddForce((Vector2.right + Vector2.up) * _knockbackForce, ForceMode2D.Impulse);
        else
            _rb.AddForce((Vector2.left + Vector2.up) * _knockbackForce, ForceMode2D.Impulse);
        
        if(Health < 1)
        {
            _anim.SetTrigger(_deathAnimHash);
            _collider.enabled = false;
            Destroy(gameObject, 2f);
        }
        else
            _anim.SetTrigger(_hitAnimHash);
    }
}
