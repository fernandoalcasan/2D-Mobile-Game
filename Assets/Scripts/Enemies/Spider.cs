using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Enemy, IDamageable
{
    public int Health { get; set; }

    private void Start()
    {
        Health = health;
    }

    public void Damage()
    {
        Health--;

        if (Health < 1)
        {
            _anim.SetTrigger(_deathAnimHash);
            //Destroy(gameObject);
        }
        else
            _anim.SetTrigger(_hitAnimHash);
    }
}
