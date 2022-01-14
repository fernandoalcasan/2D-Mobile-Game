using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackZone : MonoBehaviour
{
    [SerializeField]
    private List<Enemy> _enemiesInZone;

    private Transform _player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if (_player is null)
                _player = other.transform;

            for (int i = _enemiesInZone.Count - 1; i >= 0; i--)
            {
                if (_enemiesInZone[i])
                    _enemiesInZone[i].IdentifyPlayer(_player);
                else
                    _enemiesInZone.RemoveAt(i);
            }

            if (_enemiesInZone.Count == 0)
                Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            for (int j = _enemiesInZone.Count - 1; j >= 0; j--)
            {
                if (_enemiesInZone[j])
                    _enemiesInZone[j].ReturnToPatrol();
                else
                    _enemiesInZone.RemoveAt(j);
            }

            if (_enemiesInZone.Count == 0)
                Destroy(gameObject);
        }
        //To avoid enemies getting out of the attack zone
        else if(other.CompareTag("Enemy"))
        {
            if(other.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(other.transform.position, 10f);
            }
        }
    }
}
