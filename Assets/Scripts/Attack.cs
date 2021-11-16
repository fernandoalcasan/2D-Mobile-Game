using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Attack : MonoBehaviour
{
    [SerializeField]
    private float _resetAttackTime;
    [SerializeField]
    private float _attackPower;

    private WaitForSeconds _wait;
    private bool _canDamage = true;

    void Start()
    {
        _wait = new WaitForSeconds(_resetAttackTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_canDamage)
            return;

        if(other.TryGetComponent(out IDamageable hit))
        {
            hit.Damage(transform.parent.position, _attackPower);
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        _canDamage = false;
        yield return _wait;
        _canDamage = true;
    }
}
